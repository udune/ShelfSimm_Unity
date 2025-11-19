using TMPro;
using UnityEngine;
using Data;
using Core.Core;

namespace UI
{
    public class CellInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cellCodeText;
        [SerializeField] private TextMeshProUGUI accessibilityText;
        [SerializeField] private TextMeshProUGUI dimensionsText;     // 치수 정보 (AC-12.1)
        [SerializeField] private TextMeshProUGUI capacityText;       // 용량 정보 (AC-12)
        [SerializeField] private TextMeshProUGUI bookInfoText;       // 보관 도서 정보
        [SerializeField] private GameObject panelObject;

        [Header("Colors")]
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;

        private void Start()
        {
            // 초기에는 패널 숨김
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }

        public void UpdateCellInfo(string cellCode, bool isAccessible)
        {
            // 패널 활성화
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }

            // 셀 코드 표시
            if (cellCodeText != null)
            {
                cellCodeText.text = $"셀: {cellCode}";
            }

            // 접근성 표시
            if (accessibilityText != null)
            {
                if (isAccessible)
                {
                    accessibilityText.text = "접근 가능";
                    accessibilityText.color = accessibleColor;
                }
                else
                {
                    accessibilityText.text = "접근 불가";
                    accessibilityText.color = blockedColor;
                }
            }

            Debug.Log($"[CellInfoPanel] 셀 정보 업데이트: {cellCode} | 접근: {isAccessible}");
        }

        /// <summary>
        /// Cell 객체를 받아 상세 정보를 표시 (AC-12, AC-12.1)
        /// </summary>
        /// <param name="cell">Cell 객체</param>
        /// <param name="isAccessible">접근 가능 여부</param>
        public void UpdateCellInfoDetailed(Cell cell, bool isAccessible)
        {
            if (cell == null)
            {
                Hide();
                return;
            }

            // 패널 활성화
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }

            // 셀 코드 표시
            if (cellCodeText != null)
            {
                cellCodeText.text = $"셀: {cell.CellCode}";
            }

            // 접근성 표시
            if (accessibilityText != null)
            {
                if (isAccessible)
                {
                    accessibilityText.text = "접근 가능";
                    accessibilityText.color = accessibleColor;
                }
                else
                {
                    accessibilityText.text = "접근 불가";
                    accessibilityText.color = blockedColor;
                }
            }

            // AC-12.1: 치수 정보 (mm 단위 명시)
            if (dimensionsText != null)
            {
                dimensionsText.text = $"치수: {cell.WidthMm}mm × {cell.HeightMm}mm";
            }

            // AC-12: 용량 정보 (현재/최대)
            if (capacityText != null)
            {
                if (cell.MaxCapacity > 0)
                {
                    int remainingCapacity = cell.MaxCapacity - cell.CurrentStock;
                    capacityText.text = $"용량: {cell.CurrentStock}/{cell.MaxCapacity}권 (잔여: {remainingCapacity}권)";
                }
                else
                {
                    capacityText.text = "용량: 빈 칸 (도서 입고 시 계산됨)";
                }
            }

            // 보관 도서 정보
            if (bookInfoText != null)
            {
                if (!string.IsNullOrEmpty(cell.StoredBookTitle))
                {
                    bookInfoText.text = $"보관 도서: '{cell.StoredBookTitle}'";
                }
                else
                {
                    bookInfoText.text = "보관 도서: 없음";
                }
            }

            Debug.Log($"[CellInfoPanel] 상세 정보 업데이트: {ErrorMessageFormatter.FormatCellInfo(cell.CellCode, cell.WidthMm, cell.HeightMm, cell.CurrentStock, cell.MaxCapacity, cell.StoredBookTitle)}");
        }

        public void Hide()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }
    }
}
