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
        [SerializeField] private GridRenderer gridRenderer;
        [SerializeField] private RawImage gridImage;
        [SerializeField] private int cellSize = 10;
        [SerializeField] private CellsLayoutSO cellsLayout;

        [SerializeField] private CellInfoPanel infoPanel;

        [SerializeField] private CellHighlightManager highlightManager;
        private int gridWidth;
        private int gridHeight;

        private const int DEFAULT_GRID_SIZE = 50; // 기본 그리드 크기 (fallback)

        private void Start()
        {
            if (gridRenderer == null)
            {
                gridRenderer = FindObjectOfType<GridRenderer>();
            }

            if (gridImage == null)
            {
                gridImage = GetComponent<RawImage>();
            }

            if (highlightManager == null)
            {
                highlightManager = FindObjectOfType<CellHighlightManager>();
            }

            if (gridRenderer != null && gridRenderer.Width > 0 && gridRenderer.Height > 0)
            {
                gridWidth = gridRenderer.Width;
                gridHeight = gridRenderer.Height;
            }
            else
            {
                gridWidth = DEFAULT_GRID_SIZE;
                gridHeight = DEFAULT_GRID_SIZE;
                Debug.LogWarning($"[GridClickHandler] Using default grid size: {DEFAULT_GRID_SIZE}x{DEFAULT_GRID_SIZE}");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (gridImage == null || gridRenderer == null)
            {
                return;
            }

            Vector2Int gridPos = GetGridPosition(eventData);

            if (gridPos.x < 0 || gridPos.y < 0)
            {
                return;
            }

            string cellType = gridRenderer.GetCellType(gridPos.x, gridPos.y);
            CellDef cellDef = GetCellDefAtPosition(gridPos.x, gridPos.y);
            bool isAccessible = IsCellAccessible(gridPos.x, gridPos.y, cellType);

            if (highlightManager != null)
            {
                highlightManager.SelectCell(gridImage.gameObject, isAccessible);
                PositionHighlight(gridPos);
            }

            if (infoPanel != null && cellDef != null)
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
        
            int gridX = Mathf.FloorToInt(normalizedX * gridWidth);
            int gridY = Mathf.FloorToInt(normalizedY * gridHeight);
        
            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
            {
                return new Vector2Int(-1, -1);
            }
            
            return new Vector2Int(gridX, gridY);
        }
    
        private CellDef GetCellDefAtPosition(int x, int y)
        {
            if (cellsLayout == null)
            {
                return null;
            }

            return cellsLayout.GetCellByPosition(x, y);
        }

        private Cell GetCellData(string cellCode)
        {
            if (SimulationManager.Instance == null)
            {
                return null;
            }

            return SimulationManager.Instance.GetCellByCode(cellCode);
        }

        private bool IsCellAccessible(int x, int y, string cellType)
        {
            if (cellType == "obstacle" || cellType == "bookshelf")
            {
                return false;
            }

            return true;
        }

        private void PositionHighlight(Vector2Int gridPos)
        {
            if (highlightManager == null) return;

            GameObject highlight = highlightManager.GetSelectedCell();
            if (highlight == null) return;

            RectTransform rectTransform = gridImage.rectTransform;
            Rect rect = rectTransform.rect;

            float cellWidth = rect.width / gridWidth;
            float cellHeight = rect.height / gridHeight;

            float localX = rect.x + (gridPos.x + 0.5f) * cellWidth;
            float localY = rect.y + (gridPos.y + 0.5f) * cellHeight;

            Vector3 worldPos = rectTransform.TransformPoint(new Vector2(localX, localY));

            Transform highlightTransform = highlightManager.transform.Find("HighlightBorder");
            if (highlightTransform != null)
            {
                highlightTransform.position = worldPos;
            }
        }

        public void SetGridSize(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;
        }
    }
}
