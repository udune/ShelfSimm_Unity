using System.Collections;
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
        }

        private void ShowHighlight(GameObject cell)
        {
            highlightBorder.color = highlightColor;
            highlightBorder.gameObject.SetActive(true);

            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(blinkDuration);
                highlightBorder.gameObject.SetActive(!highlightBorder.gameObject.activeSelf);
            }
        }

        public void ClearHighlight()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }

            highlightBorder.gameObject.SetActive(false);
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
