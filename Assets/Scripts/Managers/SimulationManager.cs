using System.Collections;
using System.Collections.Generic;
using System.Linq;
using API;
using Core;
using Data;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class SimulationManager : MonoBehaviour
    {
        #region Singleton

        public static SimulationManager Instance { get; private set; }

        #endregion

        #region Fields & Properties

        [Header("API 연동 설정")] [SerializeField] private bool useApiMode = true;

        [Header("내부 컴포넌트 참조")] [SerializeField]
        private RobotController robotController;

        [SerializeField] private SimpleAStarPathFinder pathFinder;
        [SerializeField] private BookRegistry bookRegistry;
        [SerializeField] private JobInputController jobInputController;
        [SerializeField] private GridRenderer gridRenderer;
        [SerializeField] private SimulationUIController simulationUIController;

        public float ElapsedTime { get; private set; }
        public float AverageTaskTime => summary != null && summary.success > 0 ? ElapsedTime / summary.success : 0;

        private Queue<Job> _jobQueue;
        private Summary summary;
        private bool _isRunning;
        private bool _isPaused;
        private string _currentRunId;
        private List<JobResult> _jobResults;
        private Dictionary<string, Cell> _cellStates;

        private const float API_JOB_PROCESSING_DELAY = 0.5f;

        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ConfigManager.Instance.SimulationConfig.OnHandleTimeChanged += HandleTimeChanged;

            InitializeSimulation();

            if (useApiMode)
            {
                StartCoroutine(InitializeAPI());
            }

            Debug.Log("시뮬레이션 준비 완료. UI에서 작업을 입력하고 실행하세요.");
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
            ConfigManager.Instance.SimulationConfig.OnHandleTimeChanged -= HandleTimeChanged;
        }

        #endregion

        #region Initialization

        private IEnumerator InitializeAPI()
        {
            Debug.Log("API 모드 초기화 중...");

            var booksLoaded = false;
            List<BookDto> loadedBookDtos = null;
            yield return ApiClient.Instance.GetAllBooks(
                bookDtos =>
                {
                    loadedBookDtos = bookDtos;
                    booksLoaded = true;
                },
                error => Debug.LogError($"책 정보 로드 실패: {error}")
            );
            if (!booksLoaded)
            {
                HandleApiInitializationFailure("API 초기화 실패: 책 정보를 가져올 수 없습니다.");
                yield break;
            }

            if (loadedBookDtos != null)
            {
                bookRegistry.LoadBooksFromApi(loadedBookDtos);
                jobInputController.RefreshBookDropdown();
            }

            yield return StartCoroutine(RestoreInventoryState());

            Debug.Log("API 초기화 완료. 책 정보 및 재고 상태 복원 완료.");
        }

        private IEnumerator RestoreInventoryState()
        {
            Debug.Log("서버에서 재고 상태를 복원합니다...");

            string latestRunId = null;
            bool runsLoaded = false;
            yield return ApiClient.Instance.GetRuns(1, 1, 
                response =>
                {
                    if (response.items != null && response.items.Length > 0)
                    {
                        latestRunId = response.items[0].id;
                    }
                    runsLoaded = true;
                },
                error =>
                {
                    Debug.LogError($"Run 목록 조회 실패: {error}");
                    runsLoaded = true;
                });

            if (!runsLoaded)
            {
                yield return new WaitUntil(() => runsLoaded);
            }

            if (string.IsNullOrEmpty(latestRunId))
            {
                Debug.Log("이전 실행 기록이 없어 재고 상태 복원을 건너뜁니다.");
                yield break;
            }

            List<JobDetailsDto> jobs = null;
            bool jobsLoaded = false;
            yield return ApiClient.Instance.GetJobsByRunId(latestRunId,
                jobList =>
                {
                    jobs = jobList;
                    jobsLoaded = true;
                },
                error =>
                {
                    Debug.LogError($"Job 목록 조회 실패 (Run ID: {latestRunId}): {error}");
                    jobsLoaded = true;
                });

            if (!jobsLoaded)
            {
                yield return new WaitUntil(() => jobsLoaded);
            }

            if (jobs == null || jobs.Count == 0)
            {
                Debug.Log("작업 기록이 없어 재고 상태 복원을 건너뜁니다.");
                yield break;
            }

            int appliedJobs = 0;
            foreach (var job in jobs.Where(j => j.result == "Success").OrderBy(j => j.id))
            {
                Cell cell = FindCellByCode(job.cellCode);
                BookData book = FindBookByTitle(job.bookTitle);

                if (cell == null || book == null) continue;

                if (job.action == "PUT")
                {
                    if(cell.CanPutBook(book, job.quantity, out _))
                    {
                        cell.PutBook(book, job.quantity);
                        book.ChangeStock(-job.quantity);
                        appliedJobs++;
                    }
                }
                else if (job.action == "PICK")
                {
                    ErrorCode errorCode; // Declare errorCode variable
                    if(cell.CanPickBook(book, job.quantity, out errorCode)) // Pass errorCode
                    {
                        cell.PickBook(book, job.quantity);
                        book.ChangeStock(job.quantity);
                        appliedJobs++;
                    }
                }
            }
            Debug.Log($"{appliedJobs}개의 작업을 적용하여 재고 상태를 복원했습니다.");
        }

        public void PrepareSimulation(List<Job> jobs)
        {
            if (useApiMode)
                StartCoroutine(PrepareSimulationWithAPI(jobs));
            else
                StartSimulationWithJobs(jobs);
        }

        private IEnumerator PrepareSimulationWithAPI(List<Job> jobs)
        {
            if (string.IsNullOrEmpty(_currentRunId))
            {
                var createRunReq = new CreateRunRequest
                {
                    randomSeed = ConfigManager.Instance.SimulationConfig.randomSeed,
                    handleTimeSec = ConfigManager.Instance.SimulationConfig.handleTime,
                    robotSpeedCellsPerSec = ConfigManager.Instance.SimulationConfig.robotSpeed,
                    topN = ConfigManager.Instance.SimulationConfig.topN
                };
                var runCreated = false;
                yield return ApiClient.Instance.CreateRun(createRunReq,
                    response =>
                    {
                        _currentRunId = response.id;
                        runCreated = true;
                    },
                    error => Debug.LogError($"Run 생성 실패: {error}")
                );
                if (!runCreated)
                {
                    Debug.LogError("Run 생성 실패");
                    yield break;
                }
            }

            var jobDtos = jobs.Select(job => new JobDto
            {
                action = job.Action.ToString(),
                cellCode = job.CellCode,
                bookTitle = job.BookTitle,
                quantity = job.Quantity
            }).ToArray();
            var createJobsReq = new CreateJobsBatchRequest
            {
                runId = _currentRunId,
                jobs = jobDtos,
                layoutId = ConfigManager.Instance.CellsLayout.layout_hash
            };

            var jobsBatched = false;
            yield return ApiClient.Instance.CreateJobsBatch(createJobsReq,
                success => { jobsBatched = true; },
                error => Debug.LogError($"Jobs 생성 실패: {error}")
            );
            if (!jobsBatched)
            {
                Debug.LogError("Jobs 생성 실패");
                yield break;
            }

            yield return new WaitForSeconds(API_JOB_PROCESSING_DELAY);

            var idsMapped = false;
            yield return ApiClient.Instance.GetRunDetails(_currentRunId,
                runDetails =>
                {
                    var serverJobs = runDetails.jobs.ToDictionary(
                        job => (job.cellCode, job.bookTitle, job.action),
                        job => job.id
                    );

                    foreach (var localJob in jobs)
                    {
                        var key = (localJob.CellCode, localJob.BookTitle, localJob.Action.ToString());
                        if (serverJobs.TryGetValue(key, out var jobId))
                        {
                            localJob.JobId = jobId;
                        }
                    }

                    idsMapped = true;
                },
                error => Debug.LogError($"Run 상세 정보 조회 실패: {error}")
            );
            if (!idsMapped)
            {
                Debug.LogError("Job ID 매핑 실패");
                yield break;
            }

            StartSimulationWithJobs(jobs);
        }

        private void InitializeSimulation()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(ConfigManager.Instance.SimulationConfig.randomSeed);

            summary = new Summary();
            _jobQueue = new Queue<Job>();
            _jobResults = new List<JobResult>();
            _isRunning = false;
            _isPaused = false;
            _currentRunId = null;
            ElapsedTime = 0f;

            ConfigManager.Instance.CellsLayout.UpdateCellPositionsFromCodes();
            InitializeGrid();

            _cellStates = new Dictionary<string, Cell>();
            if (ConfigManager.Instance.CellsLayout != null && ConfigManager.Instance.CellsLayout.cells != null)
            {
                foreach (var cellDef in ConfigManager.Instance.CellsLayout.cells)
                {
                    _cellStates[cellDef.code] = new Cell(cellDef.code, cellDef.width, cellDef.height);
                }
            }

            if (robotController != null)
            {
                robotController.Reset();
            }
        }

        private void InitializeGrid()
        {
            gridRenderer.Init();
            foreach (var cellDef in ConfigManager.Instance.CellsLayout.cells)
            {
                gridRenderer.UpdateCell(cellDef.X, cellDef.Y, "bookshelf");
                if (pathFinder != null)
                {
                    pathFinder.AddObstacle(new Vector2Int(cellDef.X, cellDef.Y));
                }
            }
            gridRenderer.UpdateCell(ConfigManager.Instance.CellsLayout.warehouse.x,
                ConfigManager.Instance.CellsLayout.warehouse.y, "empty");
            gridRenderer.RenderChanges();
        }

        #endregion

        #region Simulation Control

        public void StartSimulationWithJobs(List<Job> jobs)
        {
            Debug.Log($"[StartSimulationWithJobs] 시뮬레이션 시작, 작업 수: {jobs?.Count ?? 0}");
            InitializeSimulation();

            if (jobs == null || jobs.Count == 0)
            {
                Debug.LogWarning("시작할 작업이 없습니다.");
                return;
            }
            
            var sortedJobs = jobs.OrderBy(job => CalculatePathLength(job.CellCode)).ToList();
            Debug.Log("작업 목록을 가까운 순으로 정렬했습니다.");

            _isRunning = true;
            _isPaused = false;
            ElapsedTime = 0f;
            Time.timeScale = 1f;

            SetTotalTargets(sortedJobs.Count);
            Debug.Log($"[StartSimulationWithJobs] Summary 초기화: total={summary.total}, attempt={summary.attempt}");

            foreach (var job in sortedJobs)
            {
                _jobQueue.Enqueue(job);
                Debug.Log($"[StartSimulationWithJobs] 작업 추가: {job.Action} - {job.CellCode} - {job.BookTitle} x{job.Quantity} (경로 길이: {CalculatePathLength(job.CellCode)})");
            }

            TryProcessNextJob();
        }

        private void TryProcessNextJob()
        {
            if (!_isRunning) return;

            if (_jobQueue.Count > 0)
            {
                var nextJob = _jobQueue.Dequeue();
                Debug.Log($"[TryProcessNextJob] 다음 작업 처리 시작: {nextJob.Action} - {nextJob.CellCode} - {nextJob.BookTitle} x{nextJob.Quantity}");

                var targetCell = FindCellByCode(nextJob.CellCode);
                var targetBook = FindBookByTitle(nextJob.BookTitle);

                if (targetCell != null && targetBook != null)
                {
                    var pathLength = CalculatePathLength(nextJob.CellCode);
                    robotController.StartJob(nextJob, targetCell, targetBook, pathLength, OnJobFinished);
                }
                else
                {
                    Debug.LogError($"작업 처리 불가: Cell({nextJob.CellCode}) 또는 Book({nextJob.BookTitle})을 찾을 수 없음");
                    OnJobFinished(nextJob, ErrorCode.INVALID_CODE, null);
                }
            }
            else
            {
                Debug.Log("[TryProcessNextJob] 모든 작업 완료. 웨어하우스로 복귀합니다.");
                robotController.DoReturnToWarehouse(OnAllJobsAndReturnFinished);
            }
        }

        private void OnJobFinished(Job job, ErrorCode resultCode, JobResult jobResult)
        {
            Debug.Log($"[OnJobFinished] 작업 완료: {job.CellCode} - {job.BookTitle}, 결과: {resultCode}");
            if (jobResult != null) _jobResults.Add(jobResult);

            if (resultCode == ErrorCode.NONE)
            {
                RecordSuccess();
            }
            else
            {
                RecordFailure(resultCode);
                ShowErrorInUI(job, resultCode);
            }

            TryProcessNextJob();
        }
        
        private void OnAllJobsAndReturnFinished()
        {
            Debug.Log("모든 작업 및 복귀 완료. 시뮬레이션을 종료합니다.");
            StopSimulation();
        }

        private void ShowErrorInUI(Job job, ErrorCode errorCode)
        {
            var errorMessage = $"작업 실패 [{job.CellCode}]: {errorCode.ToMessage()}";
            simulationUIController.ShowStatus(errorMessage, Color.red);
        }

        public void StopSimulation()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _isPaused = false;

            if (robotController != null) robotController.Stop();

            while (_jobQueue.Count > 0)
            {
                OnJobFinished(_jobQueue.Dequeue(), ErrorCode.CANCELLED_BY_STOP, null);
            }

            if (useApiMode && !string.IsNullOrEmpty(_currentRunId))
            {
                StartCoroutine(SendJobResultsToAPI());
            }

            Debug.Log(summary.ToString());
            
            // Time.timeScale을 0으로 설정하면 모든 Time-based Coroutine이 멈추므로 UI 동작을 위해 주석 처리
            // Time.timeScale = 0f;

            if (simulationUIController != null)
            {
                simulationUIController.ClearJobs();
            }
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
                if (_isPaused) robotController.Pause();
                else robotController.Resume();
            }

            Debug.Log(_isPaused ? "시뮬레이션 일시정지됨." : "시뮬레이션 재개됨.");
        }

        #endregion

        #region Summary & UI

        public void SetTotalTargets(int count)
        {
            summary.total = count;
        }

        public void RecordSuccess()
        {
            summary.RecordSuccess();
            UpdateDashboard();
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            summary.RecordFailure(errorCode);
            UpdateDashboard();
        }

        private void UpdateDashboard()
        {
            SimulationUIController.Instance.UpdateDashboard(summary);
        }

        private void HandleTimeChanged(float newHandleTime)
        {
            Debug.Log($"[SimulationManager] HandleTime이 {newHandleTime}으로 변경됨을 감지했습니다.");
        }

        #endregion

        #region Helper Methods

        private Cell FindCellByCode(string code)
        {
            if (_cellStates != null && _cellStates.TryGetValue(code, out var cell))
            {
                return cell;
            }
            return null;
        }

        public Cell GetCellByCode(string code)
        {
            return FindCellByCode(code);
        }

        private BookData FindBookByTitle(string title)
        {
            return bookRegistry.GetBookByTitle(title);
        }

        private int CalculatePathLength(string cellCode)
        {
            var cellDef = ConfigManager.Instance.CellsLayout.GetCellByCode(cellCode);
            if (cellDef == null) return 0;

            var start = ConfigManager.Instance.CellsLayout.warehouse;
            var targetCellPos = new Vector2Int(cellDef.X, cellDef.Y);

            var accessiblePos = pathFinder?.FindAccessibleNeighbor(targetCellPos, start);
            if (!accessiblePos.HasValue) return 0;

            var path = pathFinder.FindPath(start, accessiblePos.Value);
            return path?.Count ?? 0;
        }

        private void HandleApiInitializationFailure(string errorMessage)
        {
            Debug.LogError(errorMessage);
            _isRunning = false;
        }

        private IEnumerator SendJobResultsToAPI()
        {
            if (_jobResults == null || _jobResults.Count == 0)
            {
                Debug.LogWarning("전송할 Job 결과가 없습니다.");
                yield break;
            }

            Debug.Log($"시뮬레이션 종료: {_jobResults.Count}개의 Job 결과를 API로 전송합니다...");

            var successCount = 0;
            var failCount = 0;

            foreach (var jobResult in _jobResults)
            {
                if (string.IsNullOrEmpty(jobResult.JobId))
                {
                    failCount++;
                    continue;
                }

                var request = new UpdateJobResultRequest
                {
                    startTs = jobResult.StartTime.ToString("o"),
                    endTs = jobResult.EndTime.ToString("o"),
                    travelTimeSec = jobResult.TravelTimeSec,
                    handleTimeSec = jobResult.HandleTimeSec,
                    totalTimeSec = jobResult.TotalTimeSec,
                    pathLengthCells = jobResult.PathLengthCells,
                    result = jobResult.Result,
                    failReason = jobResult.FailReason,
                    robotName = jobResult.RobotName
                };

                var requestCompleted = false;
                yield return ApiClient.Instance.UpdateJobResult(jobResult.JobId, request,
                    () =>
                    {
                        successCount++;
                        requestCompleted = true;
                    },
                    error =>
                    {
                        failCount++;
                        requestCompleted = true;
                        Debug.LogError($"Job 결과 전송 실패: {error}");
                    }
                );

                if (!requestCompleted)
                {
                    failCount++;
                }
            }

            Debug.Log($"Job 결과 전송 완료: 성공 {successCount}개, 실패 {failCount}개");

            var statusReq = new UpdateRunStatusRequest { status = "COMPLETED" };
            yield return ApiClient.Instance.UpdateRunStatus(_currentRunId, statusReq,
                () => Debug.Log("Run 상태가 COMPLETED로 변경되었습니다."),
                error => Debug.LogError($"Run 상태 업데이트 실패: {error}")
            );
        }

        #endregion
    }
}
