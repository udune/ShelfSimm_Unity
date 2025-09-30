using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    // 격자 내의 단일 칸 정의
    [CreateAssetMenu(fileName = "CellsLayoutSO", menuName = "Scriptable Objects/CellsLayoutSO")]
    public class CellsLayoutSO : ScriptableObject
    {
        [Header("스키마")] 
        public string schema_version = "1.0"; // 스키마 버전
        public string type = "cells_layout"; // 타입 고정
        
        [Header("격자 크기")]
        public Vector2Int grid_size = new Vector2Int(50, 50); // 격자 크기 (셀 단위)
        
        [Header("창고 위치")]
        public Vector2Int warehouse = new Vector2Int(0, 0); // 창고 위치 (격자 좌표)
        
        [Header("칸 목록")]
        public List<CellDef> cells = new List<CellDef>(); // 칸 목록
        
        [Header("캐시 무효화")]
        public string layout_hash; // 레이아웃 변경 시 자동 갱신

        public CellDef GetCellByCode(string code) // 코드로 칸 찾기
        {
            return cells.Find(c => c.code == code); // 코드로 칸 찾기
        }
        
        public CellDef GetCellByPosition(int x, int y) // 좌표로 칸 찾기
        {
            return cells.Find(c => c.x == x && c.y == y); // 좌표로 칸 찾기
        }
        
        public List<CellDef> GetAvailableCells() // 모든 칸 가져오기 (차단되지 않은 것만)
        {
            return cells.FindAll(c => !c.blocked); // 차단되지 않은 칸들 반환
        }
    }
}
