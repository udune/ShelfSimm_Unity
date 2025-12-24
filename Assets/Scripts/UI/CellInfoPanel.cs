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

            // 모든 책 정보 표시
            var allBooks = cell.GetAllBooks();
            if (allBooks.Count > 0)
            {
                StringBuilder sb = new StringBuilder("보관 도서:\n");
                foreach (var book in allBooks)
                {
                    sb.AppendLine($"  • {book.title} ({book.quantity}권)");
                }
                bookInfoText.text = sb.ToString().TrimEnd();
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
