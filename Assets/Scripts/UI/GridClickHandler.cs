using System;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("References")] 
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private RawImage gridImage;
    [SerializeField] private int cellSize = 10; // 셀 크기 (픽셀 단위)
    
    [Header("Cell Data Registry")]
    [SerializeField] private CellRegistry cellRegistry;

    private CellHighlightManager highlightManager;
    private int gridWidth;
    private int gridHeight;

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

        if (cellRegistry == null)
        {
            cellRegistry = FindObjectOfType<CellRegistry>();
        }

        gridWidth = 50;
        gridHeight = 50;
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
            Debug.Log("[GridClickHandler] 그리드 밖 클릭");
            return;
        }
        
        string cellType = gridRenderer.GetCellType(gridPos.x, gridPos.y);
        
        string cellCode = GetCellCode(gridPos.x, gridPos.y);
        
        // 4. 접근 가능 여부 판단
        bool isAccessible = IsCellAccessible(gridPos.x, gridPos.y, cellType);
            
        // 5. 하이라이트 표시
        if (highlightManager != null)
        {
            // gridImage를 가상의 "셀 오브젝트"로 전달
            highlightManager.SelectCell(gridImage.gameObject, isAccessible);
                
            // 하이라이트 위치를 그리드 좌표로 이동
            PositionHighlight(gridPos);
        }
            
        // 6. 정보 패널 업데이트
        CellInfoPanel infoPanel = FindObjectOfType<CellInfoPanel>();
        if (infoPanel != null)
        {
            infoPanel.UpdateCellInfo(cellCode, isAccessible);
        }
            
        Debug.Log($"[GridClickHandler] 클릭: ({gridPos.x}, {gridPos.y}) | 타입: {cellType} | 코드: {cellCode} | 접근: {isAccessible}");
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
    
    private string GetCellCode(int x, int y)
    {
        if (cellRegistry != null)
        {
            return cellRegistry.GetCellCode(x, y);
        }
            
        // 레지스트리가 없으면 기본 포맷 반환
        return $"Cell_{x}_{y}";
    }
    
    // 접근 가능 여부 판단
    private bool IsCellAccessible(int x, int y, string cellType)
    {
        // 장애물이나 책장은 접근 불가
        if (cellType == "obstacle" || cellType == "bookshelf")
        {
            return false;
        }
            
        // 셀 레지스트리에서 접근성 확인
        if (cellRegistry != null)
        {
            return cellRegistry.IsAccessible(x, y);
        }
            
        return true;
    }
        
    // 하이라이트를 그리드 좌표에 맞춰 위치 조정
    private void PositionHighlight(Vector2Int gridPos)
    {
        if (highlightManager == null) return;
            
        // 하이라이트 테두리 찾기
        GameObject highlight = highlightManager.GetSelectedCell();
        if (highlight == null) return;
            
        RectTransform rectTransform = gridImage.rectTransform;
        Rect rect = rectTransform.rect;
            
        // 그리드 좌표를 RawImage 내 로컬 좌표로 변환
        float cellWidth = rect.width / gridWidth;
        float cellHeight = rect.height / gridHeight;
            
        float localX = rect.x + (gridPos.x + 0.5f) * cellWidth;
        float localY = rect.y + (gridPos.y + 0.5f) * cellHeight;
            
        // 월드 좌표로 변환
        Vector3 worldPos = rectTransform.TransformPoint(new Vector2(localX, localY));
            
        // 하이라이트 위치 설정
        Transform highlightTransform = highlightManager.transform.Find("HighlightBorder");
        if (highlightTransform != null)
        {
            highlightTransform.position = worldPos;
        }
    }
        
    // 그리드 크기 설정 (GridRenderer 초기화 후 호출)
    public void SetGridSize(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        Debug.Log($"[GridClickHandler] 그리드 크기 설정: {width}x{height}");
    }
}
