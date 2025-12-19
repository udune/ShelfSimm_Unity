using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class CellRegistry : MonoBehaviour
    {
        [System.Serializable]
        public class CellData
        {
            public string code;
            public int x;
            public int y;
            public bool isAccessible = true;
        }

        [SerializeField] private List<CellData> cells = new List<CellData>();

        private Dictionary<Vector2Int, CellData> cellLookup = new Dictionary<Vector2Int, CellData>();

        private void Awake()
        {
            BuildLookupTable();
        }

        private void BuildLookupTable()
        {
            cellLookup.Clear();

            foreach (var cell in cells)
            {
                Vector2Int pos = new Vector2Int(cell.x, cell.y);
                if (!cellLookup.ContainsKey(pos))
                {
                    cellLookup[pos] = cell;
                }
                else
                {
                    Debug.LogWarning($"[CellRegistry] 중복된 좌표: ({cell.x}, {cell.y})");
                }
            }
        }

        public string GetCellCode(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);

            if (cellLookup.TryGetValue(pos, out CellData cellData))
            {
                return cellData.code;
            }

            return $"Cell_{x}_{y}";
        }

        public bool IsAccessible(int x, int y)
        {
            Vector2Int pos = new Vector2Int(x, y);

            if (cellLookup.TryGetValue(pos, out CellData cellData))
            {
                return cellData.isAccessible;
            }

            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildLookupTable();
            }
        }
    }
}
