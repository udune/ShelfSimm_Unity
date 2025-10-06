using System.Collections.Generic;
using Core;
using Data;
using UnityEngine;

public class TestNearestSelector : MonoBehaviour
{
    public CellsLayoutSO layout; // 칸 레이아웃 데이터
    public NearestCellSelector selector; // NearestCellSelector 컴포넌트
    
    private void Start()
    {
        Vector2Int robotPos = Vector2Int.zero; // 로봇 위치 (0,0)
        
        List<CellDef> allCells = layout.GetAvailableCells(); // 사용 가능한 모든 칸 가져오기
        
        var top3 = selector.FilterTopN(robotPos, allCells); // 가장 가까운 3개의 칸 선택
        
        Debug.Log($"선택된 칸 개수: {top3.Count}");
        
        foreach (var info in top3) // 각 선택된 칸 출력
        {
            Debug.Log($"칸 {info.cell.code} at ({info.cell.x}, {info.cell.y}) - 거리: {info.distance}");
        }
    }
}
