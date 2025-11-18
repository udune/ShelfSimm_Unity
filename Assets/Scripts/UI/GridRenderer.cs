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

            // WebGL 메모리 최적화 참고:
            // 큰 그리드(예: 50x50 셀, 10픽셀 = 500x500 텍스처)는 상당한 메모리를 사용할 수 있습니다.
            // WebGL 환경에서는 메모리 제한이 더 엄격하므로, 큰 그리드 사용 시 주의가 필요합니다.
            // 필요시 cellSize를 줄이거나 텍스처 풀링을 고려하세요.
            gridTexture = new Texture2D(texWidth, texHeight);
            gridTexture.filterMode = FilterMode.Point; // 픽셀 아트 스타일

            if (gridImage != null)
            {
                gridImage.texture = gridTexture;
            }

            // 전체 초기화
            ClearGrid();

            Debug.Log($"{gridWidth}x{gridHeight} 그리드 생성 (텍스처 {texWidth}x{texHeight})");
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