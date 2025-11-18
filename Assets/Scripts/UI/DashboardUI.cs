using Managers.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DashboardUI : MonoBehaviour
    {
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private TextMeshProUGUI pauseResumeButtonText;
        [SerializeField] private Button stopButton;

        private bool isPaused = false;

        void Start()
        {
            pauseResumeButton.onClick.AddListener(TogglePauseResume);
            stopButton.onClick.AddListener(StopSimulation);
        
            // 초기 버튼 텍스트 설정
            if(pauseResumeButtonText != null)
            {
                pauseResumeButtonText.text = "중지";
            }
        }

        private void TogglePauseResume()
        {
            isPaused = !isPaused;
            SimulationManager.Instance.TogglePause();
        
            if(pauseResumeButtonText != null)
            {
                pauseResumeButtonText.text = isPaused ? "재개" : "중지";
            }
        }

        private void StopSimulation()
        {
            SimulationManager.Instance.StopSimulation();
        }
    }
}
