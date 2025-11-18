using Data;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Summary UI")]
        [SerializeField] private GameObject summaryPanel; // 시뮬레이션 종료 후 요약 팝업
        [SerializeField] private TextMeshProUGUI summaryText; // 시뮬레이션 종료 후 요약 텍스트

        [Header("Real-time Dashboard UI")]
        [SerializeField] private TextMeshProUGUI completedCountText; // 완료 건수
        [SerializeField] private TextMeshProUGUI elapsedTimeText;    // 경과 시간
        [SerializeField] private TextMeshProUGUI averageTimeText;    // 평균 처리 시간

        [Header("Error UI (Optional)")]
        [SerializeField] private GameObject errorPanel; // 에러 표시 팝업 (선택 사항)
        [SerializeField] private TextMeshProUGUI errorText; // 에러 메시지 텍스트

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

        // 시뮬레이션 종료 시 최종 요약 정보를 표시하는 메서드
        public void ShowSummary(Summary summary)
        {
            if (summaryPanel == null || summaryText == null) return;
            
            summaryPanel.SetActive(true);
            summaryText.text = summary.ToString();
        }

        // 실시간 대시보드 UI를 업데이트하는 메서드
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

        // 에러 메시지를 표시하는 메서드
        public void ShowError(string errorMessage)
        {
            Debug.LogError(errorMessage);

            // 에러 패널이 설정되어 있으면 UI로 표시
            if (errorPanel != null && errorText != null)
            {
                errorPanel.SetActive(true);
                errorText.text = errorMessage;
            }
            // 에러 패널이 없으면 summary 패널을 재사용 (fallback)
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
