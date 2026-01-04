using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class GridRenderer : MonoBehaviour
{
    [Header("그리드 설정")]
    private RawImage gridImage;

    [SerializeField]
    private int totalColumns = 15;

    [SerializeField]
    private int totalRows = 15;

    private readonly Dictionary<Vector2Int, string> cellStates = new();
    private readonly HashSet<Vector2Int> dirtyPixels = new();

    private Texture2D gridTexture;
    private int cellWidth;
    private int cellHeight;

    public int Width { get; private set; }

    public int Height { get; private set; }

    private void Start()
    {
        gridImage = GetComponent<RawImage>();
    }

    public void Init()
    {
        var rect = gridImage.rectTransform.rect;
        Width = Mathf.RoundToInt(rect.width);
        Height = Mathf.RoundToInt(rect.height);
        
        cellWidth = Width / totalColumns;
        cellHeight = Height / totalRows;

        Debug.Log($"GridRenderer initialized: {Width}x{Height} pixels, grid {totalColumns}x{totalRows}, cell size {cellWidth}x{cellHeight}");

        gridTexture = new Texture2D(Width, Height);
        gridTexture.filterMode = FilterMode.Point;

        gridImage.texture = gridTexture;

        ClearGrid();
    }

    private void ClearGrid()
    {
        var transparent = new Color(0, 0, 0, 0);
        var pixels = new Color[gridTexture.width * gridTexture.height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = transparent;
        }
        gridTexture.SetPixels(pixels);
        gridTexture.Apply();
    }

    public void UpdateCell(int x, int y, string type)
    {
        if (x < 0 || x >= totalColumns || y < 0 || y >= totalRows)
        {
            return;
        }

        var pos = new Vector2Int(x, y);
        cellStates[pos] = type;
        dirtyPixels.Add(pos);
    }

    public void RenderChanges()
    {
        if (dirtyPixels.Count == 0)
        {
            return;
        }

        foreach (var pos in dirtyPixels)
        {
            DrawCell(pos.x, pos.y, cellStates[pos]);
        }

        gridTexture.Apply();
        dirtyPixels.Clear();
    }

    private void DrawCell(int x, int y, string type)
    {
        var color = GetColor(type);

        var startX = x * cellWidth;
        var startY = y * cellHeight;

        for (var py = 0; py < cellHeight; py++)
        {
            for (var px = 0; px < cellWidth; px++)
            {
                gridTexture.SetPixel(startX + px, startY + py, color);
            }
        }
    }

    public string GetCellType(int x, int y)
    {
        var pos = new Vector2Int(x, y);
        return cellStates.ContainsKey(pos) ? cellStates[pos] : "empty";
    }

    public int TotalColumns => totalColumns;
    public int TotalRows => totalRows;
    public int CellWidth => cellWidth;
    public int CellHeight => cellHeight;

    private Color GetColor(string type)
    {
        return type switch
        {
            "empty" => new Color(0, 0, 0, 0),
            "partial" => Color.yellow,
            "full" => Color.red,
            "obstacle" => new Color(0.2f, 0.2f, 0.2f),
            "materialshelf" => new Color(0.55f, 0.43f, 0.39f),
            "robot" => Color.blue,
            _ => Color.white
        };
    }
}
}