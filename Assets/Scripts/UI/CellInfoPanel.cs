using TMPro;
using UnityEngine;
using Data;
using Core;
using System.Text;

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

        [Header("Colors")]
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void UpdateCellInfo(string cellCode, bool isAccessible)
        {
            gameObject.SetActive(true);
            cellCodeText.text = $"셀: {cellCode}";

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

        public void UpdateCellInfoDetailed(Cell cell, bool isAccessible)
        {
            if (cell == null)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);
            cellCodeText.text = $"셀: {cell.CellCode}";

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

            dimensionsText.text = $"치수: {cell.WidthMm}mm × {cell.HeightMm}mm";

            if (cell.CurrentStock > 0 && !string.IsNullOrEmpty(cell.StoredBookTitle))
            {
                bookInfoText.text = $"보관 도서: {cell.StoredBookTitle}";
            }
            else
            {
                bookInfoText.text = "보관 도서: 없음";
            }

            capacityText.text = cell.MaxCapacity > 0 ? $"용량: {cell.CurrentStock}/{cell.MaxCapacity}권" : "용량: -";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
