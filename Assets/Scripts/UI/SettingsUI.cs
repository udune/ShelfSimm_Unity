using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // SettingsUI는 시뮬레이션 설정을 위한 UI를 관리하는 클래스입니다.
    public class SettingsUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TMP_InputField handleTimeInput; // 작업 처리 시간 입력 필드
        [SerializeField] private Button applyButton; // 적용 버튼
        [SerializeField] private TextMeshProUGUI statusText; // 상태 텍스트

        private void Start()
        {
            if (applyButton != null) // 적용 버튼이 할당된 경우
            {
                applyButton.onClick.AddListener(OnApplySettings); // 적용 버튼 클릭 이벤트에 메서드 등록
            }
            
            LoadCurrentSettings(); // 현재 설정 로드
        }

        private void LoadCurrentSettings() // 현재 설정을 UI에 로드하는 메서드
        {
            if (SimulationManager.Instance != null && handleTimeInput != null) // SimulationManager와 입력 필드가 할당된 경우
            {
                float currentValue = SimulationManager.Instance.GetHandleTime(); // 현재 작업 처리 시간 가져오기
                handleTimeInput.text = currentValue.ToString("F1"); // 입력 필드에 현재 값 설정
            }
        }
        
        private void OnApplySettings() // 설정 적용 버튼 클릭 시 호출되는 메서드
        {
            if (handleTimeInput == null || SimulationManager.Instance == null) // 입력 필드나 SimulationManager가 할당되지 않은 경우 종료
            {
                return; // 아무 작업도 수행하지 않음
            }

            if (float.TryParse(handleTimeInput.text, out float newValue)) // 입력된 값을 float로 변환 시도
            {
                if (newValue > 0) // 유효한 값인지 확인 (0보다 커야 함)
                {
                    SimulationManager.Instance.UpdateHandleTime(newValue); // SimulationManager에 새로운 값 적용
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

        private void ShowStatus(string message, Color color) // 상태 메시지를 표시하는 메서드
        {
            if (statusText != null) // 상태 텍스트가 할당된 경우
            {
                statusText.text = message; // 상태 메시지 설정
                statusText.color = color; // 상태 메시지 색상 설정
                Invoke(nameof(ClearStatus), 2f); // 2초 후에 상태 메시지 지우기
            }
        }
        
        private void ClearStatus() // 상태 메시지를 지우는 메서드
        {
            if (statusText != null) // 상태 텍스트가 할당된 경우
            {
                statusText.text = ""; // 상태 메시지 지우기
            }
        }
    }
}