using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimulationConfig config;

        [Header("UI Components")]
        [SerializeField] private TMP_InputField handleTimeInput;
        [SerializeField] private Button applyButton;
        [SerializeField] private TextMeshProUGUI statusText;

        private void Start()
        {
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(OnApplySettings);
            }
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (config != null && handleTimeInput != null)
            {
                handleTimeInput.text = config.handleTime.ToString("F1");
            }
        }
        
        private void OnApplySettings()
        {
            if (config == null || handleTimeInput == null) return;

            if (float.TryParse(handleTimeInput.text, out float newValue))
            {
                if (newValue > 0)
                {
                    config.handleTime = newValue;
                    ShowStatus($"설정 적용됨: 작업 처리 시간 = {newValue:F1}초", Color.green);
                }
                else
                {
                    ShowStatus("오류: 작업 처리 시간은 0보다 커야 합니다.", Color.red);
                }
            }
            else
            {
                ShowStatus("오류: 유효한 숫자를 입력하세요.", Color.red);
            }
        }

        private void ShowStatus(string message, Color color)
        {
            if (statusText == null) return;
            statusText.text = message;
            statusText.color = color;
            Invoke(nameof(ClearStatus), 2f);
        }
        
        private void ClearStatus()
        {
            if (statusText != null)
            {
                statusText.text = "";
            }
        }
    }
}
