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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            if (summaryPanel != null)
            {
                summaryPanel.SetActive(false);
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

        private string FormatTime(float timeInSeconds)
        {
            int minutes = (int)timeInSeconds / 60;
            int seconds = (int)timeInSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
