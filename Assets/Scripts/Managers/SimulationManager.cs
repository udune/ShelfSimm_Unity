using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using API;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    /// <summary>
    /// 시뮬레이션의 전체 흐름(시작, 정지, 일시정지)을 관리하고,
    /// 작업(Job)을 순서대로 RobotController에게 할당하는 오케스트레이터 클래스입니다.
    /// API 모드에서는 서버와의 통신을 통해 시뮬레이션의 시작과 끝, 개별 작업 결과를 보고합니다.
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        #region Singleton
        
        public static SimulationManager Instance { get; private set; }
        
        #endregion

        #region Serialized Fields
        
        [Header("핵심 설정")]
        [SerializeField] private SimulationConfig config;

        [Header("API 연동 설정")]
        [SerializeField] private bool useApiMode = true;
        
        [Header("내부 컴포넌트 참조")]
        [SerializeField] private RobotController robotController;
        [SerializeField] private ApiClient apiClient;
        
        // TODO: 임시 데이터. 실제로는 데이터 관리 시스템에서 가져와야 합니다.
        [SerializeField] private List<Cell> allCells; 
        [SerializeField] private List<Book> allBooks;
        
        #endregion

        #region Public Properties
        
        public float ElapsedTime { get; private set; }

        public float AverageTaskTime
        {
            get
            {
                if (_summary != null && _summary.success > 0)
                {
                    return ElapsedTime / _summary.success;
                }
                return 0;
            }
        }
        
        #endregion

        #region Private Fields
        
        private Queue<Job> _jobQueue;
        private Summary _summary;
        private bool _isRunning;
        private bool _isPaused;
        private string _currentRunId;
        
        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (config != null)
            {
                config.OnHandleTimeChanged += HandleTimeChanged;
            }
        }

        private void Start()
        {
            if (apiClient == null)
            {
                apiClient = FindObjectOfType<ApiClient>();
            }

            if (useApiMode && apiClient != null)
            {
                StartCoroutine(InitializeWithAPI());
            }
            else
            {
                Debug.Log("로컬 모드로 시뮬레이션을 시작합니다.");
                InitializeSimulation();
                StartSimulationWithJobs(GetTestJobs());
            }
        }

        private void Update()
        {
            if (_isRunning && !_isPaused)
            {
                ElapsedTime += Time.deltaTime;
                UpdateDashboard();
            }
        }

        private void OnDestroy()
        {
            if (config != null)
            {
                config.OnHandleTimeChanged -= HandleTimeChanged;
            }
        }
        
        #endregion

        #region Initialization

        private IEnumerator InitializeWithAPI()
        {
            Debug.Log("API 모드로 시뮬레이션 초기화를 시작합니다...");

            var createRunReq = new CreateRunRequest
            {
                randomSeed = config.randomSeed,
                handleTimeSec = config.handleTime,
                robotSpeedCellsPerSec = config.robotSpeed,
                topN = config.topN
            };

            bool runCreated = false;
            yield return apiClient.CreateRun(createRunReq,
                onSuccess: response => {
                    _currentRunId = response.id;
                    runCreated = true;
                    Debug.Log($"Run 생성됨: {_currentRunId}");
                },
                onError: error => Debug.LogError($"Run 생성 실패: {error}")
            );

            if (!runCreated)
            {
                Debug.LogError("API 초기화 실패. 시뮬레이션을 중단합니다.");
                yield break;
            }

            var localJobs = GetTestJobs();
            var jobDtos = localJobs.Select(job => new API.JobDto
            {
                action = job.Action.ToString(),
                cellCode = job.CellCode,
                bookTitle = job.BookTitle,
                quantity = job.Quantity
            }).ToArray();

            var createJobsReq = new CreateJobsBatchRequest
            {
                runId = _currentRunId,
                jobs = jobDtos
            };

            bool jobsCreated = false;
            yield return apiClient.CreateJobsBatch(createJobsReq,
                onSuccess: response => {
                    jobsCreated = true;
                    Debug.Log($"{response.accepted}개 작업이 서버에 등록되었습니다.");
                },
                onError: error => Debug.LogError($"Jobs 생성 실패: {error}")
            );
            
            if (!jobsCreated)
            {
                Debug.LogError("API 작업 생성 실패. 시뮬레이션을 중단합니다.");
                yield break;
            }

            InitializeSimulation();
            StartSimulationWithJobs(localJobs);
        }
        
        private void InitializeSimulation()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(config.randomSeed);
            
            _summary = new Summary();
            _jobQueue = new Queue<Job>();
            _isRunning = true;
            _isPaused = false;
            ElapsedTime = 0f;
            
            // TODO: 임시 데이터 초기화. 실제로는 외부(예: 파일, 서버)에서 로드해야 합니다.
            allCells = new List<Cell> { new Cell("A01", 100, 120), new Cell("B02", 80, 150) };
            allBooks = new List<Book> { new Book("Test Book A", 30, 100), new Book("Test Book B", 25, 130) };
        }
        
        #endregion

        #region Job & Simulation Flow

        public void StartSimulationWithJobs(List<Job> jobs)
        {
            if (jobs == null || jobs.Count == 0)
            {
                Debug.LogWarning("시작할 작업이 없습니다.");
                return;
            }

            SetTotalTargets(jobs.Count);
            foreach (var job in jobs)
            {
                _jobQueue.Enqueue(job);
            }
            
            TryProcessNextJob();
        }

        private void TryProcessNextJob()
        {
            if (_jobQueue.Count > 0)
            {
                Job nextJob = _jobQueue.Dequeue();
                
                Cell targetCell = FindCellByCode(nextJob.CellCode);
                Book targetBook = FindBookByTitle(nextJob.BookTitle);

                if (targetCell != null && targetBook != null)
                {
                    robotController.StartJob(nextJob, targetCell, targetBook, OnJobFinished);
                }
                else
                {
                    Debug.LogError($"작업 처리 불가: Cell({nextJob.CellCode}) 또는 Book({nextJob.BookTitle})을 찾을 수 없음");
                    OnJobFinished(nextJob, ErrorCode.INVALID_CODE);
                }
            }
            else
            {
                Debug.Log("모든 작업이 큐에서 처리되었습니다.");
                CheckSimulationComplete();
            }
        }

        private void OnJobFinished(Job job, ErrorCode resultCode)
        {
            if (resultCode == ErrorCode.NONE)
            {
                RecordSuccess();
            }
            else
            {
                RecordFailure(resultCode);
            }
            
            TryProcessNextJob();
        }
        
        private void CheckSimulationComplete()
        {
            if (_summary == null)
            {
                return;
            }
            
            if (_summary.totalTargets > 0 && _summary.attempted >= _summary.totalTargets)
            {
                StopSimulation();
            }
        }

        #endregion

        #region Simulation Control

        public void StopSimulation()
        {
            if (!_isRunning)
            {
                return;
            }
            
            _isRunning = false;
            Debug.Log("시뮬레이션을 중지합니다.");
            
            if (robotController != null)
            {
                robotController.Stop();
            }

            while (_jobQueue.Count > 0)
            {
                OnJobFinished(_jobQueue.Dequeue(), ErrorCode.CANCELLED_BY_STOP);
            }

            if (useApiMode && apiClient != null && !string.IsNullOrEmpty(_currentRunId))
            {
                var statusReq = new UpdateRunStatusRequest { status = "COMPLETED" };
                StartCoroutine(apiClient.UpdateRunStatus(_currentRunId, statusReq));
            }

            Debug.Log(_summary.ToString());
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSummary(_summary);
            }
            
            Time.timeScale = 0f;
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            Debug.Log(_isPaused ? "시뮬레이션 일시정지됨." : "시뮬레이션 재개됨.");
        }
        
        #endregion

        #region Data & Summary Management

        public void SetTotalTargets(int count)
        {
            if (_summary != null)
            {
                _summary.totalTargets = count;
            }
        }
        
        public void RecordSuccess()
        {
            if (_summary != null)
            {
                _summary.RecordSuccess();
            }
            UpdateDashboard();
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            if (_summary != null)
            {
                _summary.RecordFailure(errorCode);
            }
            UpdateDashboard();
        }

        public Summary GetSummary()
        {
            return _summary;
        }
        
        #endregion

        #region UI & Event Handlers

        private void UpdateDashboard()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateDashboard(_summary);
            }
        }

        private void HandleTimeChanged(float newHandleTime)
        {
            Debug.Log($"[SimulationManager] HandleTime이 {newHandleTime}으로 변경됨을 감지했습니다.");
        }
        
        #endregion

        #region Helper & Test Methods
        
        /// <summary>
        /// 제공된 코드로 `allCells` 리스트에서 일치하는 Cell 객체를 찾습니다.
        /// </summary>
        /// <param name="code">찾을 Cell의 코드</param>
        /// <returns>일치하는 Cell 객체. 없으면 null을 반환합니다.</returns>
        private Cell FindCellByCode(string code)
        {
            // LINQ의 Find 메서드를 사용하여 조건에 맞는 첫 번째 요소를 찾습니다.
            return allCells.Find(c => c.CellCode == code);
        }

        /// <summary>
        /// 제공된 제목으로 `allBooks` 리스트에서 일치하는 Book 객체를 찾습니다.
        /// </summary>
        /// <param name="title">찾을 Book의 제목</param>
        /// <returns>일치하는 Book 객체. 없으면 null을 반환합니다.</returns>
        private Book FindBookByTitle(string title)
        {
            return allBooks.Find(b => b.Title == title);
        }

        private List<Job> GetTestJobs()
        {
            return new List<Job>
            {
                new Job(Data.JobAction.PUT, "A01", "Test Book A", 2),
                new Job(Data.JobAction.PUT, "B02", "Test Book B", 3),
                new Job(Data.JobAction.PICK, "A01", "Test Book A", 1)
            };
        }
        
        #endregion
    }
}
