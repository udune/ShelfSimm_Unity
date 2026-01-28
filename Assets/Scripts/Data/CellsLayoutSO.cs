using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CellsLayoutSO", menuName = "Scriptable Objects/CellsLayoutSO")]
    public class CellsLayoutSO : ScriptableObject
    {
        [Header("창고 위치")]
        public Vector2Int warehouse = new Vector2Int(0, 0);

        [Header("칸 목록")]
        public List<CellDef> cells = new List<CellDef>();


        public void UpdateCellPositionsFromCodes()
        {
            foreach (var cell in cells)
            {
                cell.SetPositionFromCode();
            }
        }

        private void OnValidate()
        {
            UpdateCellPositionsFromCodes();
        }

        public CellDef GetCellByCode(string code)
        {
            return cells.Find(cell => cell.code == code);
        }

        public CellDef GetCellByPosition(int x, int y)
        {
            return cells.Find(cell => cell.X == x && cell.Y == y);
        }
    }
}
