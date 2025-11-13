using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using API; // API 네임스페이스 추가
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [Header("API 연동")]
        [SerializeField] private ApiClient apiClient;
        [SerializeField] private bool useApiMode = true;

        [Header("시뮬레이션 설정")]
        [SerializeField] private SimulationConfig config;
        
        [Header("내부 참조")]
        [SerializeField] private RobotController robotController;
        [SerializeField] private List<Cell> allCells; 
        [SerializeField] private List<Book> allBooks;

        private Queue<Job> jobQueue;
        private Summary summary;
        private bool isRunning;
        private string currentRunId;
        private Dictionary<string, Job> jobIdMap = new Dictionary<string, Job>(); // Job ID로 Job 정보 추적

        public float ElapsedTime { get; private set; }
        public float AverageTaskTime => summary.success > 0 ? ElapsedTime / summary.success : 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (apiClient == null) apiClient = FindObjectOfType<ApiClient>();

            if (useApiMode && apiClient != null)
            {
                StartCoroutine(InitializeWithAPI());
            }
            else
            {
                Debug.Log("로컬 모드로 시뮬레이션을 시작합니다.");
                InitializeSimulation();
                StartTestSimulation(); // 로컬 테스트용
            }
        }

        private IEnumerator InitializeWithAPI()
        {
            Debug.Log("API 모드로 시뮬레이션 초기화를 시작합니다...");

            // 1. Run 생성
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
                    currentRunId = response.id;
                    Debug.Log($"Run 생성됨: {currentRunId}");
                    runCreated = true;
                },
                onError: error => Debug.LogError($"Run 생성 실패: {error}")
            );

            if (!runCreated)
            {
                Debug.LogError("API 초기화 실패. 시뮬레이션을 중단합니다.");
                yield break;
            }

            // 2. 작업 목록 생성 (예시 데이터 사용)
            var jobsToCreate = GetTestJobs(); // API로 보낼 작업 목록
            var jobDtos = jobsToCreate.Select(job => new API.JobDto
            {
                action = job.Action.ToString(),
                cellCode = job.CellCode,
                bookTitle = job.BookTitle,
                quantity = job.Quantity
            }).ToArray();

            var createJobsReq = new CreateJobsBatchRequest
            {
                runId = currentRunId,
                jobs = jobDtos
            };

            bool jobsCreated = false;
            yield return apiClient.CreateJobsBatch(createJobsReq,
                onSuccess: response => {
                    Debug.Log($"{response.accepted}개 작업이 서버에 등록되었습니다.");
                    jobsCreated = true;
                },
                onError: error => Debug.LogError($"Jobs 생성 실패: {error}")
            );
            
            if (!jobsCreated)
            {
                Debug.LogError("API 작업 생성 실패. 시뮬레이션을 중단합니다.");
                yield break;
            }

            // 3. 시뮬레이션 시작
            InitializeSimulation();
            StartSimulationWithJobs(jobsToCreate);
        }

        private void Update()
        {
            if (isRunning && !isPaused)
            {
                ElapsedTime += Time.deltaTime;
                UpdateDashboard();
            }
        }

        private void InitializeSimulation()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(config.randomSeed);
            summary = new Summary();
            jobQueue = new Queue<Job>();
            isRunning = true;
            
            allCells = new List<Cell> { new Cell("A01", 100, 120), new Cell("B02", 80, 150) };
            allBooks = new List<Book> { new Book("Test Book A", 30, 100), new Book("Test Book B", 25, 130) };
        }

        public void StartSimulationWithJobs(List<Job> jobs)
        {
            if (jobs == null || jobs.Count == 0) return;

            SetTotalTargets(jobs.Count);
            foreach (var job in jobs)
            {
                jobQueue.Enqueue(job);
            }
            
            TryProcessNextJob();
        }

        private void TryProcessNextJob()
        {
            if (jobQueue.Count > 0)
            {
                Job nextJob = jobQueue.Dequeue();
                
                Cell targetCell = allCells.Find(c => c.CellCode == nextJob.CellCode);
                Book targetBook = allBooks.Find(b => b.Title == nextJob.BookTitle);

                if (targetCell != null && targetBook != null)
                {
                    robotController.StartJob(nextJob, targetCell, targetBook, (resultCode) => OnJobFinished(nextJob, resultCode));
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
            if (resultCode == ErrorCode.NONE) RecordSuccess();
            else RecordFailure(resultCode);

            if (useApiMode && apiClient != null && !string.IsNullOrEmpty(currentRunId))
            {
                // TODO: Job ID를 받아와야 함. 현재는 CellCode를 임시 ID로 사용
                string jobId = job.CellCode; 
                var resultReq = new UpdateJobResultRequest
                {
                    result = (resultCode == ErrorCode.NONE) ? "SUCCESS" : "FAIL",
                    failReason = (resultCode != ErrorCode.NONE) ? resultCode.ToString() : null,
                    // TODO: 시간 및 경로 데이터 채우기
                };
                StartCoroutine(apiClient.UpdateJobResult(jobId, resultReq));
            }
            
            TryProcessNextJob();
        }
        
        private void StartTestSimulation()
        {
            StartSimulationWithJobs(GetTestJobs());
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

        public void SetTotalTargets(int count)
        {
            if (summary != null) summary.totalTargets = count;
        }
        
        public void RecordSuccess()
        {
            if (summary != null) summary.RecordSuccess();
            UpdateDashboard();
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            if (summary != null) summary.RecordFailure(errorCode);
            UpdateDashboard();
        }
        
        private void CheckSimulationComplete()
        {
            if (summary == null) return;
            
            if (summary.totalTargets > 0 && summary.attempted >= summary.totalTargets)
            {
                StopSimulation();
            }
        }

        public void StopSimulation()
        {
            if (!isRunning) return;
            
            isRunning = false;
            Debug.Log("Simulation Stopped.");
            
            if (robotController != null) robotController.Stop();

            while (jobQueue.Count > 0)
            {
                var cancelledJob = jobQueue.Dequeue();
                OnJobFinished(cancelledJob, ErrorCode.CANCELLED_BY_STOP);
            }

            if (useApiMode && apiClient != null && !string.IsNullOrEmpty(currentRunId))
            {
                var statusReq = new UpdateRunStatusRequest { status = "COMPLETED" };
                StartCoroutine(apiClient.UpdateRunStatus(currentRunId, statusReq));
            }

            Debug.Log(summary.ToString());
            if (UIManager.Instance != null) UIManager.Instance.ShowSummary(summary);
            Time.timeScale = 0f;
        }
        
        public Summary GetSummary() => summary;

        private void UpdateDashboard()
        {
            if (UIManager.Instance != null && summary != null)
            {
                UIManager.Instance.UpdateDashboard(summary);
            }
        }

        private bool isPaused;
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (robotController != null)
            {
                if (isPaused) robotController.Pause();
                else robotController.Resume();
            }
        }

        public void UpdateHandleTime(float newValue)
        {
            if (newValue > 0 && config != null)
            {
                config.handleTime = newValue;
                if (robotController != null) robotController.UpdateHandleTime(newValue);
            }
        }

        public float GetHandleTime() => config.handleTime;
    }
}
