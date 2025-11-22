using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GridRenderer : MonoBehaviour
    {
        [Header("그리드 설정")]
        [SerializeField] private RawImage gridImage;
        [SerializeField] private int cellSize = 10; // 픽셀 단위

        private Texture2D gridTexture;
        private int width;
        private int height;
        private readonly Dictionary<Vector2Int, string> cellStates = new();
        private readonly HashSet<Vector2Int> dirtyPixels = new();

        public int Width => width;
        public int Height => height;

        public void Init(int gridWidth, int gridHeight)
        {
            width = gridWidth;
            height = gridHeight;

            var texWidth = gridWidth * cellSize;
            var texHeight = gridHeight * cellSize;

            gridTexture = new Texture2D(texWidth, texHeight);
            gridTexture.filterMode = FilterMode.Point;

            if (gridImage != null)
            {
                gridImage.texture = gridTexture;
            }

            ClearGrid();
        }

        private void ClearGrid()
        {
            var emptyColor = GetColor("empty");
            var pixels = new Color[gridTexture.width * gridTexture.height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = emptyColor;
            }
            gridTexture.SetPixels(pixels);
            gridTexture.Apply();
        }

        public void UpdateCell(int x, int y, string type)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
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

            // 셀 영역 채우기
            var startX = x * cellSize;
            var startY = y * cellSize;

            for (var py = 0; py < cellSize; py++)
            {
                for (var px = 0; px < cellSize; px++)
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
    
        private Color GetColor(string type)
        {
            return type switch
            {
                "empty" => new Color(0.9f, 0.9f, 0.9f),
                "partial" => Color.yellow,
                "full" => Color.red,
                "obstacle" => new Color(0.2f, 0.2f, 0.2f),
                "bookshelf" => new Color(0.55f, 0.43f, 0.39f),
                "robot" => Color.blue,
                _ => Color.white
            };
        }
    }
}