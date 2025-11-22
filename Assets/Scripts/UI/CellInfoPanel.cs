using TMPro;
using UnityEngine;
using Data;
using Core;

namespace UI
{
    public class CellInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cellCodeText;
        [SerializeField] private TextMeshProUGUI accessibilityText;
        [SerializeField] private TextMeshProUGUI dimensionsText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private TextMeshProUGUI bookInfoText;
        [SerializeField] private GameObject panelObject;

        [Header("Colors")]
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;

        private void Start()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }

        public void UpdateCellInfo(string cellCode, bool isAccessible)
        {
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }

            if (cellCodeText != null)
            {
                cellCodeText.text = $"셀: {cellCode}";
            }

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
        }

        public void UpdateCellInfoDetailed(Cell cell, bool isAccessible)
        {
            if (cell == null)
            {
                Hide();
                return;
            }

            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }

            if (cellCodeText != null)
            {
                cellCodeText.text = $"셀: {cell.CellCode}";
            }

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

            if (dimensionsText != null)
            {
                dimensionsText.text = $"치수: {cell.WidthMm}mm × {cell.HeightMm}mm";
            }

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
