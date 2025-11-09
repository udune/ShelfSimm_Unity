using System.Collections.Generic;
using UnityEngine;

public class CellRegistry : MonoBehaviour
{
    [System.Serializable]
    public class CellData
    {
        public string code;           // 셀 코드 (예: D20, A15)
        public int x;                 // 그리드 X 좌표
        public int y;                 // 그리드 Y 좌표
        public bool isAccessible = true; // 접근 가능 여부
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

        Debug.Log($"[CellRegistry] {cellLookup.Count}개 셀 등록 완료");
    }

    public string GetCellCode(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (cellLookup.TryGetValue(pos, out CellData cellData))
        {
            return cellData.code;
        }

        // 등록되지 않은 셀은 기본 포맷 반환
        return $"Cell_{x}_{y}";
    }

    public bool IsAccessible(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (cellLookup.TryGetValue(pos, out CellData cellData))
        {
            return cellData.isAccessible;
        }

        // 등록되지 않은 셀은 기본적으로 접근 가능
        return true;
    }

    // Inspector에서 셀 데이터 수정 후 호출 (선택사항)
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            BuildLookupTable();
        }
    }
}
