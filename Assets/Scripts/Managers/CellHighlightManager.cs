using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class CellHighlightManager : MonoBehaviour
    {
        [Header("Highlight Settings")] 
        [SerializeField] private Image highlightBorder;
        [SerializeField] private float blinkDuration = 0.3f;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Accessibility Badge")]
        [SerializeField] private GameObject accessibilityBadge;
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private Image badgeBackground;
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;
        
        private bool isBlinking = false;
        private GameObject currentSelectedCell;

        public void SelectCell(GameObject cell, bool isAccessible)
        {
            if (cell == null)
            {
                return;
            }

            if (currentSelectedCell != cell && currentSelectedCell != null)
            {
                ClearHighlight();
            }

            currentSelectedCell = cell;

            ShowHighlight(cell);

            ShowAccessibilityBadge(isAccessible);
        }

        private void ShowHighlight(GameObject cell)
        {
            if (highlightBorder == null)
            {
                return;
            }

            highlightBorder.transform.position = cell.transform.position;
            highlightBorder.color = highlightColor;
            highlightBorder.gameObject.SetActive(true);

            // 이미 깜빡이고 있다면 중지
            if (isBlinking)
            {
                CancelInvoke(nameof(ToggleHighlight));
            }

            isBlinking = true;
            // 0.3초 간격으로 깜빡임 시작
            InvokeRepeating(nameof(ToggleHighlight), blinkDuration, blinkDuration);
        }

        private void ToggleHighlight()
        {
            if (highlightBorder == null)
            {
                return;
            }

            // 가시성 토글 (visible ↔ invisible)
            highlightBorder.gameObject.SetActive(!highlightBorder.gameObject.activeSelf);
        }

        private void ShowAccessibilityBadge(bool isAccessible)
        {
            if (accessibilityBadge == null || badgeText == null || badgeBackground == null)
            {
                return;
            }
            
            accessibilityBadge.SetActive(true);

            if (isAccessible)
            {
                badgeText.text = "접근 가능";
                badgeBackground.color = accessibleColor;
            }
            else
            {
                badgeText.text = "접근 불가";
                badgeBackground.color = blockedColor;
            }
        }
        
        public void ClearHighlight()
        {
            if (isBlinking)
            {
                CancelInvoke(nameof(ToggleHighlight));
                isBlinking = false;
            }

            if (highlightBorder != null)
            {
                highlightBorder.gameObject.SetActive(false);
            }

            if (accessibilityBadge != null)
            {
                accessibilityBadge.SetActive(false);
            }

            currentSelectedCell = null;
        }

        public GameObject GetSelectedCell()
        {
            return currentSelectedCell;
        }
    }
}
