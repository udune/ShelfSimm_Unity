using System.Collections;
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

        private Coroutine blinkCoroutine;
        private GameObject currentSelectedCell;

        public void SelectCell(GameObject cell, bool isAccessible)
        {
            if (cell == null)
            {
                ClearHighlight();
                return;
            }

            if (currentSelectedCell != cell)
            {
                ClearHighlight();
                currentSelectedCell = cell;
                ShowHighlight(cell);
            }

            ShowAccessibilityBadge(isAccessible);
        }

        private void ShowHighlight(GameObject cell)
        {
            if (highlightBorder == null)
            { return; }

            highlightBorder.transform.position = cell.transform.position;
            highlightBorder.color = highlightColor;
            highlightBorder.gameObject.SetActive(true);

            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(blinkDuration);
                if (highlightBorder != null)
                {
                    highlightBorder.gameObject.SetActive(!highlightBorder.gameObject.activeSelf);
                }
            }
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
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
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

        private void OnDisable()
        {
            ClearHighlight();
        }

        public GameObject GetSelectedCell()
        {
            return currentSelectedCell;
        }
    }
}
