using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API.API;
using Core.Core;
using Data.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers.Managers
{
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
        [SerializeField] private SimpleAStarPathFinder pathFinder;
        [SerializeField] private CellsLayoutSO cellsLayout;

        [Header("임시 데이터")]
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

            // 1. 책 정보 로드
            bool booksLoaded = false;
            yield return apiClient.GetAllBooks(
                onSuccess: bookDtos => {
                    allBooks = bookDtos.Select(dto => new Book(dto.title, dto.thicknessMm, dto.heightMm)).ToList();
                    booksLoaded = true;
                },
                onError: error => Debug.LogError($"책 정보 로드 실패: {error}")
            );
            if (!booksLoaded)
            {
                HandleApiInitializationFailure("API 초기화 실패: 책 정보를 가져올 수 없습니다.");
                yield break;
            }

            // 2. Run 생성
            var createRunReq = new CreateRunRequest
            {
                randomSeed = config.randomSeed,
                handleTimeSec = config.handleTime,
                robotSpeedCellsPerSec = config.robotSpeed,
                topN = config.topN
            };
            bool runCreated = false;
            yield return apiClient.CreateRun(createRunReq,
                onSuccess: response => { _currentRunId = response.id; runCreated = true; },
                onError: error => Debug.LogError($"Run 생성 실패: {error}")
            );
            if (!runCreated)
            {
                HandleApiInitializationFailure("API 초기화 실패: Run을 생성할 수 없습니다.");
                yield break;
            }

            // 3. Job 일괄 생성
            var localJobs = GetTestJobs();
            var jobDtos = localJobs.Select(job => new JobDto
            {
                action = job.Action.ToString(),
                cellCode = job.CellCode,
                bookTitle = job.BookTitle,
                quantity = job.Quantity
            }).ToArray();
            var createJobsReq = new CreateJobsBatchRequest { runId = _currentRunId, jobs = jobDtos };

            bool jobsBatched = false;
            yield return apiClient.CreateJobsBatch(createJobsReq,
                onSuccess: response => { jobsBatched = true; },
                onError: error => Debug.LogError($"Jobs 생성 실패: {error}")
            );
            if (!jobsBatched)
            {
                HandleApiInitializationFailure("API 초기화 실패: Jobs를 생성할 수 없습니다.");
                yield break;
            }

            // [오류 수정 2] 서버가 Jobs를 처리할 시간을 주기 위해 짧은 대기시간 추가
            yield return new WaitForSeconds(0.5f);

            // 4. Job ID 매핑을 위해 Run 상세 정보 다시 요청
            bool idsMapped = false;
            yield return apiClient.GetRunDetails(_currentRunId,
                onSuccess: runDetails => {
                    var serverJobs = runDetails.jobs.ToDictionary(
                        j => (j.cellCode, j.bookTitle, j.action), 
                        j => j.id
                    );

                    foreach (var localJob in localJobs)
                    {
                        var key = (localJob.CellCode, localJob.BookTitle, localJob.Action.ToString());
                        if (serverJobs.TryGetValue(key, out string jobId))
                        {
                            localJob.JobId = jobId;
                        }
                        else
                        {
                            Debug.LogWarning($"서버에서 해당 Job의 ID를 찾을 수 없습니다: {key}");
                        }
                    }
                    idsMapped = true;
                    Debug.Log("Job ID 매핑 완료.");
                },
                onError: error => Debug.LogError($"Run 상세 정보 조회 실패: {error}")
            );
            if (!idsMapped)
            {
                HandleApiInitializationFailure("API 초기화 실패: Job ID를 매핑할 수 없습니다.");
                yield break;
            }

            // 5. 모든 API 통신이 성공하면, 로컬 시뮬레이션을 시작합니다.
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
            
            // API 모드가 아닐 경우를 대비해 임시 데이터 초기화
            if (allBooks == null || allBooks.Count == 0)
            {
                allBooks = new List<Book> { new Book("Test Book A", 30, 100), new Book("Test Book B", 25, 130) };
            }
            if (allCells == null || allCells.Count == 0)
            {
                allCells = new List<Cell> { new Cell("A01", 100, 120), new Cell("B02", 80, 150) };
            }
        }
        
        #endregion

        #region Job & Simulation Flow

        public void StartSimulationWithJobs(List<Job> jobs)
        {
            // [오류 수정 1] 시뮬레이션이 초기화되지 않은 상태에서 호출될 경우를 대비한 안전장치
            if (_jobQueue == null || _summary == null)
            {
                Debug.LogWarning("시뮬레이션이 초기화되지 않았습니다. 지금 초기화를 진행합니다.");
                InitializeSimulation();
            }

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
                    int pathLength = CalculatePathLength(nextJob.CellCode);
                    robotController.StartJob(nextJob, targetCell, targetBook, pathLength, OnJobFinished);
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
            _isPaused = false; // 일시정지 상태 초기화
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
            if (!_isRunning)
            {
                Debug.LogWarning("시뮬레이션이 실행 중이 아니므로 일시정지/재개할 수 없습니다.");
                return;
            }

            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;

            if (robotController != null)
            {
                if (_isPaused)
                {
                    robotController.Pause();
                }
                else
                {
                    robotController.Resume();
                }
            }

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
        
        private Cell FindCellByCode(string code)
        {
            return allCells.Find(c => c.CellCode == code);
        }

        private Book FindBookByTitle(string title)
        {
            return allBooks.Find(b => b.Title == title);
        }

        private List<Job> GetTestJobs()
        {
            return new List<Job>
            {
                new Job(JobAction.PUT, "A01", "Test Book A", 2),
                new Job(JobAction.PUT, "B02", "Test Book B", 3),
                new Job(JobAction.PICK, "A01", "Test Book A", 1)
            };
        }

        private int CalculatePathLength(string cellCode)
        {
            if (pathFinder == null || cellsLayout == null)
            {
                return 0;
            }

            CellDef cellDef = cellsLayout.GetCellByCode(cellCode);
            if (cellDef == null)
            {
                return 0;
            }

            Vector2Int start = cellsLayout.warehouse;
            Vector2Int goal = new Vector2Int(cellDef.x, cellDef.y);

            List<Vector2Int> path = pathFinder.FindPath(start, goal);
            return path != null ? path.Count : 0;
        }

        private void HandleApiInitializationFailure(string errorMessage)
        {
            Debug.LogError(errorMessage);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowError(errorMessage);
            }

            _isRunning = false;
        }

        #endregion
    }
}
