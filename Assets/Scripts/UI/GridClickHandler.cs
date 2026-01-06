using System;
using Data;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class GridClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        private GridRenderer gridRenderer;
        private RawImage gridImage;
        [SerializeField] private Transform highlightBorder;

        [SerializeField] private CellInfoPanel infoPanel;

        [SerializeField] private CellHighlightManager highlightManager;

        private const int TOTAL_COLUMNS = 15;
        private const int TOTAL_ROWS = 15;

        private void Start()
        {
            gridImage = GetComponent<RawImage>();
            gridRenderer = GetComponent<GridRenderer>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2Int gridPos = GetGridPosition(eventData);

            if (gridPos.x < 0 || gridPos.y < 0)
            {
                return;
            }

            string cellType = gridRenderer.GetCellType(gridPos.x, gridPos.y);
            CellDef cellDef = GetCellDefAtPosition(gridPos.x, gridPos.y);
            bool isAccessible = IsCellAccessible(gridPos.x, gridPos.y, cellType);
            
            highlightManager.SelectCell(gridImage.gameObject, isAccessible);
            PositionHighlight(gridPos);

            if (cellDef != null)
            {
                Cell cell = GetCellData(cellDef.code);
                if (cell != null)
                {
                    infoPanel.UpdateCellInfoDetailed(cell, isAccessible);
                }
                else
                {
                    infoPanel.UpdateCellInfo(cellDef.code, isAccessible);
                }
            }
        }

        private Vector2Int GetGridPosition(PointerEventData eventData)
        {
            RectTransform rectTransform = gridImage.rectTransform;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint))
            {
                return new Vector2Int(-1, -1);
            }

            Rect rect = rectTransform.rect;

            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;

            int gridX = Mathf.FloorToInt(normalizedX * TOTAL_COLUMNS);
            int gridY = Mathf.FloorToInt(normalizedY * TOTAL_ROWS);

            if (gridX < 0 || gridX >= TOTAL_COLUMNS || gridY < 0 || gridY >= TOTAL_ROWS)
            {
                return new Vector2Int(-1, -1);
            }

            return new Vector2Int(gridX, gridY);
        }
    
        private CellDef GetCellDefAtPosition(int x, int y)
        {
            return ConfigManager.Instance.CellsLayout.GetCellByPosition(x, y);
        }

        private Cell GetCellData(string cellCode)
        {
            return SimulationManager.Instance.GetCellByCode(cellCode);
        }

        private bool IsCellAccessible(int x, int y, string cellType)
        {
            if (cellType == "obstacle" || cellType == "materialshelf")
            {
                return false;
            }

            return true;
        }

        private void PositionHighlight(Vector2Int gridPos)
        {
            GameObject highlight = highlightManager.GetSelectedCell();
            if (highlight == null || highlightBorder == null)
            {
                return;
            }

            RectTransform rectTransform = gridImage.rectTransform;
            Rect rect = rectTransform.rect;

            float cellWidth = gridRenderer.CellWidth;
            float cellHeight = gridRenderer.CellHeight;

            float localX = rect.x + (gridPos.x + 0.5f) * cellWidth;
            float localY = rect.y + (gridPos.y + 0.5f) * cellHeight;

            Vector3 worldPos = rectTransform.TransformPoint(new Vector2(localX, localY));

            highlightBorder.position = worldPos;

            RectTransform highlightRect = highlightBorder.GetComponent<RectTransform>();
            highlightRect.sizeDelta = new Vector2(cellWidth, cellHeight);
        }
    }
}
