using Data;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Summary UI")]
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI summaryText;

        [Header("Real-time Dashboard UI")]
        [SerializeField] private TextMeshProUGUI completedCountText;
        [SerializeField] private TextMeshProUGUI elapsedTimeText;
        [SerializeField] private TextMeshProUGUI averageTimeText;

        [Header("Error UI (Optional)")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }

            if (errorPanel != null)
            {
                errorPanel.SetActive(false);
            }
        }

        public void ShowSummary(Summary summary)
        {
            if (summaryPanel == null || summaryText == null) return;

            summaryPanel.SetActive(true);
            summaryText.text = summary.ToString();
        }

        public void UpdateDashboard(Summary summary)
        {
            if (summary == null) return;

            var simManager = SimulationManager.Instance;
            if (simManager == null) return;

            if (completedCountText != null)
                completedCountText.text = $"완료 건수: {summary.success}";

            if (elapsedTimeText != null)
                elapsedTimeText.text = $"경과 시간: {FormatTime(simManager.ElapsedTime)}";

            if (averageTimeText != null)
                averageTimeText.text = $"평균 처리 시간: {FormatTime(simManager.AverageTaskTime)}";
        }

        public void CloseSummary()
        {
            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
            }
        }

        public void ShowError(string errorMessage)
        {
            Debug.LogError(errorMessage);

            if (errorPanel != null && errorText != null)
            {
                errorPanel.SetActive(true);
                errorText.text = errorMessage;
            }
            else if (summaryPanel != null && summaryText != null)
            {
                summaryPanel.SetActive(true);
                summaryText.text = $"[에러]\n{errorMessage}";
            }
        }

        public void CloseError()
        {
            if (errorPanel != null)
            {
                errorPanel.SetActive(false);
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
