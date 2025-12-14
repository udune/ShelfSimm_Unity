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
        [Header("핵심 설정")]
        [SerializeField] private SimulationConfig config;

        [Header("API 연동 설정")]
        [SerializeField] private bool useApiMode = true;

        [Header("내부 컴포넌트 참조")]
        [SerializeField] private RobotController robotController;
        [SerializeField] private SimpleAStarPathFinder pathFinder;
        [SerializeField] private CellsLayoutSO cellsLayout;
        [SerializeField] private BookRegistry bookRegistry;
        [SerializeField] private JobInputController jobInputController;
        [SerializeField] private GridRenderer gridRenderer;
        [SerializeField] private SimulationUIController simulationUIController;

        [Header("임시 데이터")]
        [SerializeField] private List<Cell> allCells;
        [SerializeField] private List<Book> allBooks;

        public float ElapsedTime { get; private set; }
        public float AverageTaskTime => _summary != null && _summary.success > 0 ? ElapsedTime / _summary.success : 0;

        private Queue<Job> _jobQueue;
        private Summary _summary;
        private bool _isRunning;
        private bool _isPaused;
        private string _currentRunId;

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

            if (config != null)
            {
                config.OnHandleTimeChanged += HandleTimeChanged;
            }
        }

        private void Start()
        {
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
            config.OnHandleTimeChanged -= HandleTimeChanged;
        }
        
        #endregion

        #region Initialization
        private IEnumerator InitializeAPI()
        {
            Debug.Log("API 모드 초기화 중...");

            bool booksLoaded = false;
            List<BookDto> loadedBookDtos = null;
            yield return ApiClient.Instance.GetAllBooks(
                bookDtos => {
                    allBooks = bookDtos.Select(dto => new Book($"BOOK_{dto.id}", dto.title, dto.thicknessMn, dto.heightMm)).ToList();
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

            if (bookRegistry != null && loadedBookDtos != null)
            {
                bookRegistry.LoadBooksFromApi(loadedBookDtos);
                jobInputController.RefreshBookDropdown();
            }

            Debug.Log("API 초기화 완료. 책 정보 로드 완료.");
        }

        public void PrepareSimulation(List<Job> jobs)
        {
            if (useApiMode)
            {
                StartCoroutine(PrepareSimulationWithAPI(jobs));
            }
            else
            {
                StartSimulationWithJobs(jobs);
            }
        }

        private IEnumerator PrepareSimulationWithAPI(List<Job> jobs)
        {
            if (string.IsNullOrEmpty(_currentRunId))
            {
                var createRunReq = new CreateRunRequest
                {
                    randomSeed = config.randomSeed,
                    handleTimeSec = config.handleTime,
                    robotSpeedCellsPerSec = config.robotSpeed,
                    topN = config.topN
                };
                bool runCreated = false;
                yield return ApiClient.Instance.CreateRun(createRunReq,
                    response => { _currentRunId = response.id; runCreated = true; },
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
                layoutId = cellsLayout != null ? cellsLayout.layout_hash : ""
            };

            bool jobsBatched = false;
            yield return ApiClient.Instance.CreateJobsBatch(createJobsReq,
                success =>{ jobsBatched = true; },
                error => Debug.LogError($"Jobs 생성 실패: {error}")
            );
            if (!jobsBatched)
            {
                Debug.LogError("Jobs 생성 실패");
                yield break;
            }

            yield return new WaitForSeconds(API_JOB_PROCESSING_DELAY);

            bool idsMapped = false;
            yield return ApiClient.Instance.GetRunDetails(_currentRunId,
                runDetails => {
                    var serverJobs = runDetails.jobs.ToDictionary(
                        j => (j.cellCode, j.bookTitle, j.action),
                        j => j.id
                    );

                    foreach (var localJob in jobs)
                    {
                        var key = (localJob.CellCode, localJob.BookTitle, localJob.Action.ToString());
                        if (serverJobs.TryGetValue(key, out string jobId))
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
            Random.InitState(config.randomSeed);

            _summary = new Summary();
            _jobQueue = new Queue<Job>();
            _isRunning = false;
            _isPaused = false;
            ElapsedTime = 0f;

            if (allBooks == null || allBooks.Count == 0)
            {
                allBooks = new List<Book>();
            }
            if (allCells == null || allCells.Count == 0)
            {
                allCells = new List<Cell>();
            }

            if (cellsLayout != null && cellsLayout.cells != null)
            {
                cellsLayout.UpdateCellPositionsFromCodes();
            }

            InitializeGrid();

            if (cellsLayout != null && cellsLayout.cells != null && gridRenderer != null)
            {
                foreach (var cellDef in cellsLayout.cells)
                {
                    allCells.Add(new Cell(cellDef.code, cellDef.width, cellDef.height));
                }
            }
        }

        private void InitializeGrid()
        {
            gridRenderer.Init();

            if (cellsLayout.cells != null)
            {
                foreach (var cellDef in cellsLayout.cells)
                {
                    gridRenderer.UpdateCell(cellDef.X, cellDef.Y, "bookshelf");

                    if (pathFinder != null)
                    {
                        pathFinder.AddObstacle(new Vector2Int(cellDef.X, cellDef.Y));
                    }
                }
            }

            gridRenderer.UpdateCell(cellsLayout.warehouse.x, cellsLayout.warehouse.y, "empty");
            gridRenderer.RenderChanges();
        }
        #endregion

        #region Simulation Control
        public void StartSimulationWithJobs(List<Job> jobs)
        {
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

            _isRunning = true;
            _isPaused = false;
            ElapsedTime = 0f;
            Time.timeScale = 1f;

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
                ShowErrorInUI(job, resultCode);
            }

            TryProcessNextJob();
        }

        private void ShowErrorInUI(Job job, ErrorCode errorCode)
        {
            string errorMessage = $"작업 실패 [{job.CellCode}]: {errorCode.ToMessage()}";
            simulationUIController.ShowStatus(errorMessage, Color.red);
        }

        private void CheckSimulationComplete()
        {
            if (_summary == null)
            {
                return;
            }

            if (_summary.total > 0 && _summary.attempt >= _summary.total)
            {
                StopSimulation();
            }
        }

        public void StopSimulation()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _isPaused = false;

            if (robotController != null)
            {
                robotController.Stop();
            }

            while (_jobQueue.Count > 0)
            {
                OnJobFinished(_jobQueue.Dequeue(), ErrorCode.CANCELLED_BY_STOP);
            }

            if (useApiMode && !string.IsNullOrEmpty(_currentRunId))
            {
                var statusReq = new UpdateRunStatusRequest { status = "COMPLETED" };
                StartCoroutine(ApiClient.Instance.UpdateRunStatus(_currentRunId, statusReq));
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

        #region Summary & UI
        public void SetTotalTargets(int count)
        {
            if (_summary != null)
            {
                _summary.total = count;
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

        private void UpdateDashboard()
        {
            UIManager.Instance.UpdateDashboard(_summary);
        }

        private void HandleTimeChanged(float newHandleTime)
        {
            Debug.Log($"[SimulationManager] HandleTime이 {newHandleTime}으로 변경됨을 감지했습니다.");
        }
        #endregion

        #region Helper Methods
        private Cell FindCellByCode(string code)
        {
            return allCells.Find(c => c.CellCode == code);
        }

        public Cell GetCellByCode(string code)
        {
            return FindCellByCode(code);
        }

        public CellsLayoutSO GetCellsLayout()
        {
            return cellsLayout;
        }

        private Book FindBookByTitle(string title)
        {
            return allBooks.Find(b => b.Title == title);
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
            Vector2Int goal = new Vector2Int(cellDef.X, cellDef.Y);

            List<Vector2Int> path = pathFinder.FindPath(start, goal);
            return path != null ? path.Count : 0;
        }

        private void HandleApiInitializationFailure(string errorMessage)
        {
            Debug.LogError(errorMessage);
            UIManager.Instance.ShowError(errorMessage);
            _isRunning = false;
        }
        #endregion
    }
}
