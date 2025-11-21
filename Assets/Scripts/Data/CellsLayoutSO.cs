using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CellsLayoutSO", menuName = "Scriptable Objects/CellsLayoutSO")]
    public class CellsLayoutSO : ScriptableObject
    {
        [Header("스키마")]
        public string schema_version = "1.0";
        public string type = "cells_layout";

        [Header("격자 크기")]
        public Vector2Int grid_size = new Vector2Int(50, 50);

        [Header("창고 위치")]
        public Vector2Int warehouse = new Vector2Int(0, 0);

        [Header("칸 목록")]
        public List<CellDef> cells = new List<CellDef>();

        [Header("캐시 무효화")]
        public string layout_hash;

        public CellDef GetCellByCode(string code)
        {
            return cells.Find(c => c.code == code);
        }

        public CellDef GetCellByPosition(int x, int y)
        {
            return cells.Find(c => c.x == x && c.y == y);
        }

        public List<CellDef> GetAvailableCells()
        {
            return cells.FindAll(c => !c.blocked);
        }
    }
}
