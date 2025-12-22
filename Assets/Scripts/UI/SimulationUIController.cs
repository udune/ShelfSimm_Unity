using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimulationUIController : MonoBehaviour
    {
        public static SimulationUIController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private JobInputController jobInputController;
        [SerializeField] private BookRegistry bookRegistry;

        [Header("Job List UI")]
        [SerializeField] private Transform jobListContainer;
        [SerializeField] private GameObject jobItemPrefab;
        [SerializeField] private TextMeshProUGUI jobCountText;

        [Header("Control Buttons")]
        [SerializeField] private Button addJobButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Button startSimulationButton;

        [Header("Dashboard Buttons")]
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private TextMeshProUGUI pauseResumeButtonText;
        [SerializeField] private Button stopButton;

        [Header("Dashboard UI")]
        [SerializeField] private TextMeshProUGUI completedCountText;
        [SerializeField] private TextMeshProUGUI elapsedTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;

        private List<Job> jobList = new List<Job>();
        private bool isPaused = false;

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
            pauseResumeButton.onClick.AddListener(TogglePauseResume);
            stopButton.onClick.AddListener(StopSimulation);

            pauseResumeButtonText.text = "중지";

            UpdateUI();
        }

        private void OnJobAdded(JobInputData jobInput)
        {
            var bookData = bookRegistry != null ? bookRegistry.GetBookById(jobInput.bookId) : null;
            string bookTitle = bookData != null ? bookData.title : "Unknown Book";

            var invalidCells = new List<string>();
            int addedCount = 0;

            foreach (var cellCode in jobInput.parsedCodes)
            {
                if (!IsValidBookshelfCell(cellCode))
                {
                    invalidCells.Add(cellCode);
                    continue;
                }

                var job = new Job(jobInput.actionType, cellCode, bookTitle, jobInput.quantity);
                jobList.Add(job);
                addedCount++;
            }

            UpdateJobList();
            jobInputController.ResetInput();

            if (invalidCells.Count > 0)
            {
                string message = $"{addedCount}개 추가됨. 책장에 등록되지 않은 셀: {string.Join(", ", invalidCells)}";
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

        private bool IsValidBookshelfCell(string cellCode)
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
                texts[0].text = $"{index + 1}. {job.Action} - {job.CellCode} - {job.BookTitle} x{job.Quantity}";
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

        private void TogglePauseResume()
        {
            isPaused = !isPaused;
            SimulationManager.Instance.TogglePause();
            pauseResumeButtonText.text = isPaused ? "재개" : "중지";
        }

        private void StopSimulation()
        {
            SimulationManager.Instance.StopSimulation();
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
