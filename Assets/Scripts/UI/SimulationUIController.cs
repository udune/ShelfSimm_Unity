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
        [Header("References")]
        [SerializeField] private SimulationManager simulationManager;
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

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;

        private List<Job> jobList = new List<Job>();

        private void Start()
        {
            if (simulationManager == null)
            {
                simulationManager = SimulationManager.Instance;
            }

            if (jobInputController != null)
            {
                jobInputController.OnExecuteRequested += OnJobAdded;
            }

            if (addJobButton != null)
            {
                addJobButton.onClick.AddListener(OnAddJobClicked);
            }

            if (clearAllButton != null)
            {
                clearAllButton.onClick.AddListener(OnClearAllClicked);
            }

            if (startSimulationButton != null)
            {
                startSimulationButton.onClick.AddListener(OnStartSimulationClicked);
            }

            UpdateUI();
        }

        private void OnJobAdded(JobInputData jobInput)
        {
            if (bookRegistry == null)
            {
                bookRegistry = FindObjectOfType<BookRegistry>();
            }

            var bookData = bookRegistry != null ? bookRegistry.GetBookById(jobInput.bookId) : null;
            string bookTitle = bookData != null ? bookData.title : "Unknown Book";

            foreach (var cellCode in jobInput.parsedCodes)
            {
                var job = new Job(jobInput.actionType, cellCode, bookTitle, jobInput.quantity);
                jobList.Add(job);
            }

            UpdateJobList();
            jobInputController.ResetInput();

            ShowStatus($"{jobInput.parsedCodes.Count}개의 작업이 추가되었습니다.", Color.green);
        }

        private void OnAddJobClicked()
        {
            if (jobInputController != null)
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

            if (simulationManager != null)
            {
                simulationManager.PrepareSimulation(new List<Job>(jobList));
                ShowStatus($"{jobList.Count}개의 작업으로 시뮬레이션을 시작합니다.", Color.green);
            }
            else
            {
                ShowStatus("SimulationManager를 찾을 수 없습니다.", Color.red);
            }
        }

        private void UpdateJobList()
        {
            if (jobListContainer != null)
            {
                foreach (Transform child in jobListContainer)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < jobList.Count; i++)
                {
                    CreateJobItem(jobList[i], i);
                }
            }

            UpdateUI();
        }

        private void CreateJobItem(Job job, int index)
        {
            if (jobItemPrefab == null || jobListContainer == null)
            {
                return;
            }

            GameObject item = Instantiate(jobItemPrefab, jobListContainer);
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 1)
            {
                string actionIcon = job.Action == JobAction.PUT ? "📥" : "📤";
                texts[0].text = $"{index + 1}. {actionIcon} {job.Action} - {job.CellCode} - {job.BookTitle} x{job.Quantity}";
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
            if (jobCountText != null)
            {
                jobCountText.text = $"작업 목록 ({jobList.Count}개)";
            }

            if (startSimulationButton != null)
            {
                startSimulationButton.interactable = jobList.Count > 0;
            }
        }

        private void ShowStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
                Invoke(nameof(ClearStatus), 3f);
            }
        }

        private void ClearStatus()
        {
            if (statusText != null)
            {
                statusText.text = "";
            }
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
    }
}
