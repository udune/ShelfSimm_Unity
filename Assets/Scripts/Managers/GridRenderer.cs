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

    [Header("셀 스프라이트")]
    [SerializeField] private Sprite materialShelfSprite;
    [SerializeField] private Sprite robotSprite;

    private readonly Dictionary<Vector2Int, string> cellStates = new();
    private readonly HashSet<Vector2Int> dirtyPixels = new();
    private readonly Dictionary<string, Color[]> spritePixelCache = new();

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

        CacheSpritePixels();
        ClearGrid();
    }

    private void CacheSpritePixels()
    {
        spritePixelCache.Clear();

        CacheSpriteForType("materialshelf", materialShelfSprite);
        CacheSpriteForType("robot", robotSprite);
    }

    private void CacheSpriteForType(string type, Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite for type '{type}' is not assigned.");
            return;
        }

        try
        {
            Texture2D spriteTexture = sprite.texture;
            Rect spriteRect = sprite.textureRect;

            int spriteWidth = Mathf.RoundToInt(spriteRect.width);
            int spriteHeight = Mathf.RoundToInt(spriteRect.height);

            Color[] originalPixels = spriteTexture.GetPixels(
                Mathf.RoundToInt(spriteRect.x),
                Mathf.RoundToInt(spriteRect.y),
                spriteWidth,
                spriteHeight
            );

            Color[] scaledPixels = ScalePixels(originalPixels, spriteWidth, spriteHeight, cellWidth, cellHeight);
            spritePixelCache[type] = scaledPixels;

            Debug.Log($"Cached sprite pixels for '{type}': {spriteWidth}x{spriteHeight} -> {cellWidth}x{cellHeight}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to cache sprite for type '{type}': {e.Message}. Make sure the sprite's texture is set to Read/Write Enabled in Import Settings.");
        }
    }

    private Color[] ScalePixels(Color[] sourcePixels, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        Color[] scaledPixels = new Color[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (float)x / targetWidth;
                float v = (float)y / targetHeight;

                int sourceX = Mathf.FloorToInt(u * sourceWidth);
                int sourceY = Mathf.FloorToInt(v * sourceHeight);

                sourceX = Mathf.Clamp(sourceX, 0, sourceWidth - 1);
                sourceY = Mathf.Clamp(sourceY, 0, sourceHeight - 1);

                scaledPixels[y * targetWidth + x] = sourcePixels[sourceY * sourceWidth + sourceX];
            }
        }

        return scaledPixels;
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
        var startX = x * cellWidth;
        var startY = y * cellHeight;

        if (spritePixelCache.ContainsKey(type))
        {
            Color[] spritePixels = spritePixelCache[type];

            for (var py = 0; py < cellHeight; py++)
            {
                for (var px = 0; px < cellWidth; px++)
                {
                    int spriteIndex = py * cellWidth + px;
                    if (spriteIndex < spritePixels.Length)
                    {
                        gridTexture.SetPixel(startX + px, startY + py, spritePixels[spriteIndex]);
                    }
                }
            }
        }
        else
        {
            var color = GetColor(type);

            for (var py = 0; py < cellHeight; py++)
            {
                for (var px = 0; px < cellWidth; px++)
                {
                    gridTexture.SetPixel(startX + px, startY + py, color);
                }
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