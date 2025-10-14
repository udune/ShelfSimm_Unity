using System;
using Data;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Summary UI")]
        [SerializeField] private GameObject summaryPanel; // 요약 패널
        [SerializeField] private TextMeshProUGUI summaryText; // 요약
        
        [Header("Dashboard UI")]
        [SerializeField] private TextMeshProUGUI totalTargetsText; // 총 목표 수 텍스트
        [SerializeField] private TextMeshProUGUI successText; // 성공한 수
        [SerializeField] private TextMeshProUGUI failedText; // 실패한 수

        private void Awake()
        {
            if (Instance == null) // Instance가 아직 할당되지 않은 경우
            {
                Instance = this; // 현재 인스턴스를 할당
            }
            else
            {
                Destroy(gameObject); // 이미 인스턴스가 존재하면 현재 게임 오브젝트를 파괴
            }

            if (summaryPanel != null) // 요약 패널이 할당된 경우
            {
                summaryPanel.SetActive(false); // 시작 시 비활성화
            }
        }

        public void ShowSummary(Summary summary) // 시뮬레이션 요약 정보 표시 메서드
        {
            // null 체크
            if (summaryPanel == null || summaryText == null)
            {
                return;
            }
            
            // 요약 패널 활성화
            summaryPanel.SetActive(true);
            
            // 대시보드 UI 업데이트
            summaryText.text = summary.ToString();
        }

        // 대시보드 UI 업데이트 메서드
        public void UpdateDashboard(Summary summary)
        {
            if (summary == null) // null 체크
            {
                return;
            }

            if (totalTargetsText != null) // 총 목표 수 텍스트가 할당된 경우
            {
                totalTargetsText.text = $"총 작업: {summary.totalTargets}";
            }
            
            if (successText != null) // 성공한 수 텍스트가 할당된 경우
            {
                successText.text = $"성공: {summary.success}";
            }
            
            if (failedText != null) // 실패한 수 텍스트가 할당된 경우
            {
                failedText.text = $"실패: {summary.failed}";
            }
        }

        public void CloseSummary() // 요약 패널 닫기 메서드
        {
            if (summaryPanel != null) // 요약 패널이 할당된 경우
            {
                summaryPanel.SetActive(false); // 요약 패널 비활성화
            }
        }
    }
}