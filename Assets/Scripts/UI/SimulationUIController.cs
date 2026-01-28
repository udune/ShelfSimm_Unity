using System.Collections;
using System.Collections.Generic;
using Core;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using API;

namespace UI
{
    public enum SimulationState
    {
        IDLE,           // 시작 전/완료 후
        RUNNING,        // 실행 중
        PAUSED,         // 일시정지
        RESTARTING      // warehouse 복귀 중
    }

    public class SimulationUIController : MonoBehaviour
    {
        public static SimulationUIController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private JobInputController jobInputController;
        [SerializeField] private MaterialRegistry materialRegistry;

        [Header("Job List UI")]
        [SerializeField] private Transform jobListContainer;
        [SerializeField] private GameObject jobItemPrefab;
        [SerializeField] private TextMeshProUGUI jobCountText;

        [Header("Control Buttons")]
        [SerializeField] private Button addJobButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Button startSimulationButton;

        [Header("Control Buttons - Separate")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button emergencyButton;

        [Header("Dashboard UI")]
        [SerializeField] private TextMeshProUGUI completedCountText;
        [SerializeField] private TextMeshProUGUI elapsedTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Material Inventory")]
        [SerializeField] private MaterialInventoryPanel materialInventoryPanel;

        [Header("Navigation Buttons (LeftArea > header > layout)")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button warehouseButton;

        [Header("Navigation Button Images")]
        [SerializeField] private GameObject homeImage;
        [SerializeField] private GameObject warehouseImage;

        private List<Job> jobList = new List<Job>();
        private SimulationState currentState = SimulationState.IDLE;
        private Coroutine restartCoroutine = null;

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
            jobInputController.OnExecuteRequested += OnJobAdded;
            addJobButton.onClick.AddListener(OnAddJobClicked);
            clearAllButton.onClick.AddListener(OnClearAllClicked);
            startSimulationButton.onClick.AddListener(OnStartSimulationClicked);

            // 새로운 버튼 리스너
            restartButton.onClick.AddListener(OnRestartClicked);
            playButton.onClick.AddListener(OnPlayClicked);
            pauseButton.onClick.AddListener(OnPauseClicked);
            emergencyButton.onClick.AddListener(OnEmergencyStopClicked);

            // 초기 상태 설정
            UpdateButtonStates(SimulationState.IDLE);
            UpdateUI();

            // Material Inventory 네비게이션 초기화
            InitializeMaterialInventory();
        }

        // ==================== Material Inventory 네비게이션 ====================

        private void InitializeMaterialInventory()
        {
            if (homeButton != null)
            {
                homeButton.onClick.AddListener(OnHomeButtonClicked);
            }
            if (warehouseButton != null)
            {
                warehouseButton.onClick.AddListener(OnWarehouseButtonClicked);
            }

            // 초기 상태: 메인 대시보드 표시 (자재 인벤토리 숨김)
            SetInventoryVisible(false);
        }

        private void OnHomeButtonClicked()
        {
            SetInventoryVisible(false);
            Debug.Log("[UI] Home clicked - Material Inventory Hidden");
        }

        private void OnWarehouseButtonClicked()
        {
            SetInventoryVisible(true);
            Debug.Log("[UI] Warehouse clicked - Material Inventory Shown");
        }

        private void SetInventoryVisible(bool visible)
        {
            // 패널 표시/숨김
            if (materialInventoryPanel != null)
            {
                if (visible)
                    materialInventoryPanel.Show();
                else
                    materialInventoryPanel.Hide();
            }

            // 버튼 이미지 상태 전환
            // visible=false (메인 대시보드): home 활성화, warehouse 비활성화
            // visible=true (자재 인벤토리): home 비활성화, warehouse 활성화
            if (homeImage != null)
                homeImage.SetActive(!visible);
            if (warehouseImage != null)
                warehouseImage.SetActive(visible);
        }

        // ==================== 버튼 이벤트 핸들러 ====================

        private void OnRestartClicked()
        {
            Debug.Log("[UI] Restart 버튼 클릭");

            // 1. 즉시 RESTARTING 상태로 전환 (모든 버튼 비활성화)
            UpdateButtonStates(SimulationState.RESTARTING);

            // 2. 시뮬레이션 중지
            SimulationManager.Instance.StopSimulation();

            // 3. API 모드면 Run 상태를 cancelled로 업데이트
            if (SimulationManager.Instance.UseApiMode &&
                !string.IsNullOrEmpty(SimulationManager.Instance.CurrentRunId))
            {
                var request = new UpdateRunStatusRequest { status = "cancelled" };
                StartCoroutine(ApiClient.Instance.UpdateRunStatus(
                    SimulationManager.Instance.CurrentRunId,
                    request,
                    onSuccess: () => Debug.Log("[API] Run cancelled successfully"),
                    onError: (err) => Debug.LogError($"[API] Run cancel failed: {err}")
                ));
            }

            // 4. 로봇을 warehouse로 복귀 (코루틴으로 처리)
            if (restartCoroutine != null)
            {
                StopCoroutine(restartCoroutine);
            }
            restartCoroutine = StartCoroutine(RestartRobotToWarehouse());
        }

        private IEnumerator RestartRobotToWarehouse()
        {
            RobotController robot = FindObjectOfType<RobotController>();
            if (robot == null)
            {
                Debug.LogError("[UI] RobotController를 찾을 수 없습니다!");
                UpdateButtonStates(SimulationState.IDLE);
                yield break;
            }

            // 현재 위치 확인
            Vector2Int currentPos = robot.CurrentPosition;
            Vector2Int warehousePos = ConfigManager.Instance.CellsLayout.warehouse;

            Debug.Log($"[UI] 로봇 복귀 시작: {currentPos} → {warehousePos}");
            ShowStatus("로봇이 warehouse로 복귀 중입니다...", Color.yellow);

            // 로봇 리셋 (warehouse로 즉시 이동)
            robot.Reset();

            // warehouse 도착까지 대기 (실제 이동 애니메이션이 있다면)
            // 현재는 Reset()이 즉시 이동시키므로 짧은 딜레이만
            yield return new WaitForSeconds(0.5f);

            // warehouse 도착 확인
            Debug.Log($"[UI] 로봇 warehouse 도착: {robot.CurrentPosition}");
            ShowStatus("시뮬레이션이 리셋되었습니다.", Color.green);

            // IDLE 상태로 전환
            UpdateButtonStates(SimulationState.IDLE);
            restartCoroutine = null;
        }

        private void OnPlayClicked()
        {
            Debug.Log($"[UI] Play 버튼 클릭 (현재 상태: {currentState})");

            if (currentState == SimulationState.IDLE)
            {
                // 시뮬레이션 시작
                OnStartSimulationClicked();
            }
            else if (currentState == SimulationState.PAUSED)
            {
                // 재개
                SimulationManager.Instance.TogglePause();
                UpdateButtonStates(SimulationState.RUNNING);
                ShowStatus("시뮬레이션이 재개되었습니다.", Color.green);
            }
        }

        private void OnPauseClicked()
        {
            Debug.Log("[UI] Pause 버튼 클릭");

            if (currentState == SimulationState.RUNNING)
            {
                SimulationManager.Instance.TogglePause();
                UpdateButtonStates(SimulationState.PAUSED);
                ShowStatus("시뮬레이션이 일시정지되었습니다.", Color.yellow);
            }
        }

        private void OnEmergencyStopClicked()
        {
            Debug.Log("[UI] Emergency Stop 버튼 클릭");

            if (currentState == SimulationState.RUNNING)
            {
                // pause와 동일한 기능
                SimulationManager.Instance.TogglePause();
                UpdateButtonStates(SimulationState.PAUSED);

                // DB에 emergency stop 이벤트 기록
                LogEmergencyStop();

                ShowStatus("⚠️ 긴급 정지되었습니다.", Color.red);
            }
        }

        private void LogEmergencyStop()
        {
            // API 모드면 emergency stop 이벤트를 DB에 기록
            if (SimulationManager.Instance.UseApiMode &&
                !string.IsNullOrEmpty(SimulationManager.Instance.CurrentRunId))
            {
                var request = new UpdateRunStatusRequest { status = "emergency_stopped" };
                StartCoroutine(ApiClient.Instance.UpdateRunStatus(
                    SimulationManager.Instance.CurrentRunId,
                    request,
                    onSuccess: () => Debug.Log("[API] Emergency stop logged to DB"),
                    onError: (err) => Debug.LogError($"[API] Emergency stop log failed: {err}")
                ));
            }

            // 로컬 로그에도 기록
            Debug.LogWarning($"[EMERGENCY STOP] Time: {System.DateTime.Now}, RunId: {SimulationManager.Instance.CurrentRunId}");
        }

        // ==================== 버튼 상태 관리 ====================

        private void UpdateButtonStates(SimulationState newState)
        {
            currentState = newState;

            switch (newState)
            {
                case SimulationState.IDLE:
                    restartButton.interactable = false;
                    playButton.interactable = jobList.Count > 0;
                    pauseButton.interactable = false;
                    emergencyButton.interactable = false;
                    Debug.Log("[UI] 상태: IDLE - play만 활성화");
                    break;

                case SimulationState.RUNNING:
                    restartButton.interactable = true;
                    playButton.interactable = false;
                    pauseButton.interactable = true;
                    emergencyButton.interactable = true;
                    Debug.Log("[UI] 상태: RUNNING - restart/pause/emergency 활성화");
                    break;

                case SimulationState.PAUSED:
                    restartButton.interactable = true;
                    playButton.interactable = true;
                    pauseButton.interactable = false;
                    emergencyButton.interactable = false;
                    Debug.Log("[UI] 상태: PAUSED - restart/play 활성화");
                    break;

                case SimulationState.RESTARTING:
                    restartButton.interactable = false;
                    playButton.interactable = false;
                    pauseButton.interactable = false;
                    emergencyButton.interactable = false;
                    Debug.Log("[UI] 상태: RESTARTING - 모든 버튼 비활성화");
                    break;
            }
        }

        // ==================== 기존 메서드들 ====================

        private void OnJobAdded(JobInputData jobInput)
        {
            // 1순위: Material ID (LotId) 검증
            var materialData = materialRegistry != null ? materialRegistry.GetMaterialByLotId(jobInput.materialId) : null;
            if (materialData == null)
            {
                ShowStatus($"존재하지 않는 자재 ID입니다: {jobInput.materialId}", Color.red);
                return;
            }

            string MaterialTitle = materialData.name;

            // 2순위: 유효기간 검증
            if (!string.IsNullOrEmpty(materialData.ExpiryDate))
            {
                if (System.DateTime.TryParse(materialData.ExpiryDate, out System.DateTime expiryDate))
                {
                    if (expiryDate < System.DateTime.Today)
                    {
                        ShowStatus($"유효기간이 지난 자재입니다: {materialData.Name} (유효기간: {materialData.ExpiryDate})", Color.red);
                        return;
                    }
                }
            }

            // 3순위: Cell code 검증
            var invalidCells = new List<string>();
            var insufficientStockCells = new List<string>();
            int addedCount = 0;

            foreach (var cellCode in jobInput.parsedCodes)
            {
                if (!IsValidMaterialshelfCell(cellCode))
                {
                    invalidCells.Add(cellCode);
                    continue;
                }

                // 4순위: PICK 작업 시 재고 검증
                if (jobInput.actionType == JobAction.PICK)
                {
                    var cell = SimulationManager.Instance.GetCellByCode(cellCode);
                    if (cell != null)
                    {
                        int currentStock = cell.GetMaterialQuantity(materialData.Id);
                        if (currentStock < jobInput.quantity)
                        {
                            insufficientStockCells.Add($"{cellCode}(재고:{currentStock})");
                            continue;
                        }
                    }
                }

                var job = new Job(jobInput.materialId, jobInput.actionType, cellCode, MaterialTitle, jobInput.quantity);
                jobList.Add(job);
                addedCount++;
            }

            UpdateJobList();
            jobInputController.ResetInput();

            // 우선순위 순서대로 에러 메시지 표시
            if (invalidCells.Count > 0)
            {
                string message = $"{addedCount}개 추가됨. 책장에 등록되지 않은 셀: {string.Join(", ", invalidCells)}";
                ShowStatus(message, Color.red);
            }
            else if (insufficientStockCells.Count > 0)
            {
                string message = $"{addedCount}개 추가됨. 재고 부족: {string.Join(", ", insufficientStockCells)}";
                ShowStatus(message, Color.red);
            }
            else if (addedCount > 0)
            {
                ShowStatus($"{addedCount}개의 작업이 추가되었습니다.", Color.green);
            }
            else
            {
                ShowStatus("추가할 수 있는 작업이 없습니다.", Color.red);
            }
        }

        private bool IsValidMaterialshelfCell(string cellCode)
        {
            return ConfigManager.Instance.CellsLayout.GetCellByCode(cellCode) != null;
        }

        private void OnAddJobClicked()
        {
            var currentInput = jobInputController.GetCurrentJobInput();
            var validation = InputValidator.ValidateJobInput(currentInput);

            if (validation.IsValid)
            {
                OnJobAdded(currentInput);
            }
            else
            {
                ShowStatus("입력이 유효하지 않습니다.", Color.red);
            }
        }

        private void OnClearAllClicked()
        {
            jobList.Clear();
            UpdateJobList();
            ShowStatus("모든 작업이 삭제되었습니다.", Color.yellow);
        }

        private void OnStartSimulationClicked()
        {
            if (jobList.Count == 0)
            {
                ShowStatus("작업이 없습니다. 작업을 추가하세요.", Color.red);
                return;
            }

            SimulationManager.Instance.PrepareSimulation(new List<Job>(jobList));
            UpdateButtonStates(SimulationState.RUNNING);
            ShowStatus($"{jobList.Count}개의 작업으로 시뮬레이션을 시작합니다.", Color.green);
        }

        private void UpdateJobList()
        {
            foreach (Transform child in jobListContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < jobList.Count; i++)
            {
                CreateJobItem(jobList[i], i);
            }

            UpdateUI();
        }

        private void CreateJobItem(Job job, int index)
        {
            GameObject item = Instantiate(jobItemPrefab, jobListContainer);
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 1)
            {
                texts[0].text = $"{job.MaterialId}";
                texts[1].text = job.Action == JobAction.PUT ? "입고" : "출고";
                texts[2].text = $"{job.MaterialName}";
                texts[3].text = $"{job.CellCode}";
                texts[4].text = $"{job.Quantity}";
            }

            var deleteButton = item.GetComponentInChildren<Button>();
            if (deleteButton != null)
            {
                int capturedIndex = index;
                deleteButton.onClick.AddListener(() => RemoveJob(capturedIndex));
            }
        }

        private void RemoveJob(int index)
        {
            if (index >= 0 && index < jobList.Count)
            {
                jobList.RemoveAt(index);
                UpdateJobList();
                ShowStatus("작업이 삭제되었습니다.", Color.yellow);
            }
        }

        private void UpdateUI()
        {
            jobCountText.text = $"작업 목록 ({jobList.Count}개)";
            startSimulationButton.interactable = jobList.Count > 0;

            // IDLE 상태에서 작업 개수 변경 시 play 버튼도 업데이트
            if (currentState == SimulationState.IDLE)
            {
                playButton.interactable = jobList.Count > 0;
            }
        }

        public void ShowStatus(string message, Color color)
        {
            statusText.text = message;
            statusText.color = color;
            Invoke(nameof(ClearStatus), 3f);
        }

        private void ClearStatus()
        {
            statusText.text = "";
        }

        public List<Job> GetJobList()
        {
            return new List<Job>(jobList);
        }

        public void ClearJobs()
        {
            jobList.Clear();
            UpdateJobList();
        }

        public void UpdateDashboard(Summary summary)
        {
            completedCountText.text = $"완료 건수: {summary.success}";
            elapsedTimeText.text = $"경과 시간: {FormatTime(SimulationManager.Instance.ElapsedTime)}";
            averageTimeText.text = $"평균 처리 시간: {FormatTime(SimulationManager.Instance.AverageTaskTime)}";
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }

        // ==================== Public 메서드 (SimulationManager에서 호출) ====================

        public void OnSimulationCompleted()
        {
            Debug.Log("[UI] 시뮬레이션 완료 - IDLE 상태로 전환");
            UpdateButtonStates(SimulationState.IDLE);
        }

        public SimulationState GetCurrentState()
        {
            return currentState;
        }
    }
}
