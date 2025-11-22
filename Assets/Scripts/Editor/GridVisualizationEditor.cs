using UnityEditor;
using UnityEngine;
using Data;
using System.Collections.Generic;

namespace Editor
{
    [InitializeOnLoad]
    public static class GridVisualizationEditor
    {
        private static bool enabled = true;
        private static CellsLayoutSO cachedLayout;
        private static Dictionary<Vector2Int, CellDef> cellLookup;

        static GridVisualizationEditor()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem("Window/ShelfSim/Toggle Grid Visualization")]
        private static void ToggleVisualization()
        {
            enabled = !enabled;
            SceneView.RepaintAll();
            Debug.Log($"Grid Visualization: {(enabled ? "Enabled" : "Disabled")}");
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!enabled) return;

            var layout = FindLayout();
            if (layout == null) return;

            Handles.BeginGUI();
            DrawLegend();
            Handles.EndGUI();

            DrawGrid(layout);
        }

        private static CellsLayoutSO FindLayout()
        {
            if (cachedLayout != null)
                return cachedLayout;

            var allLayouts = Resources.FindObjectsOfTypeAll<CellsLayoutSO>();
            if (allLayouts.Length > 0)
            {
                cachedLayout = allLayouts[0];
                BuildCellLookup();
            }

            return cachedLayout;
        }

        private static void BuildCellLookup()
        {
            cellLookup = new Dictionary<Vector2Int, CellDef>();
            if (cachedLayout == null || cachedLayout.cells == null) return;

            foreach (var cell in cachedLayout.cells)
            {
                var pos = new Vector2Int(cell.x, cell.y);
                cellLookup[pos] = cell;
            }
        }

        private static void DrawGrid(CellsLayoutSO layout)
        {
            if (layout.cells == null || layout.cells.Count == 0)
                return;

            var warehouse = layout.warehouse;
            var gridSize = layout.grid_size;

            // Draw warehouse
            DrawCube(warehouse, Color.cyan, 0.8f, "Warehouse");

            // Draw cells
            foreach (var cell in layout.cells)
            {
                var pos = new Vector2Int(cell.x, cell.y);
                Color color = cell.blocked ? Color.red : Color.green;
                float alpha = cell.blocked ? 0.3f : 0.5f;

                DrawCube(pos, color, alpha, cell.code);

                // Draw cell orientation
                if (!cell.blocked)
                {
                    DrawOrientation(pos, cell.orientation);
                }
            }

            // Draw grid bounds
            DrawGridBounds(gridSize);
        }

        private static void DrawCube(Vector2Int gridPos, Color color, float alpha, string label = "")
        {
            Vector3 worldPos = GridToWorld(gridPos);
            Color colorWithAlpha = new Color(color.r, color.g, color.b, alpha);

            Handles.color = colorWithAlpha;
            Handles.CubeHandleCap(0, worldPos, Quaternion.identity, 0.9f, EventType.Repaint);

            // Draw label
            if (!string.IsNullOrEmpty(label))
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.fontSize = 10;
                style.fontStyle = FontStyle.Bold;
                style.alignment = TextAnchor.MiddleCenter;

                Vector3 labelPos = worldPos + Vector3.up * 0.6f;
                Handles.Label(labelPos, label, style);
            }

            // Draw wireframe
            Handles.color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f);
            Handles.DrawWireCube(worldPos, Vector3.one * 0.95f);
        }

        private static void DrawOrientation(Vector2Int gridPos, string orientation)
        {
            Vector3 worldPos = GridToWorld(gridPos);
            Vector3 direction = orientation switch
            {
                "N" => Vector3.forward,
                "S" => Vector3.back,
                "E" => Vector3.right,
                "W" => Vector3.left,
                _ => Vector3.zero
            };

            if (direction != Vector3.zero)
            {
                Handles.color = Color.yellow;
                Vector3 arrowStart = worldPos;
                Vector3 arrowEnd = worldPos + direction * 0.4f;
                Handles.DrawLine(arrowStart, arrowEnd);
                Handles.ConeHandleCap(0, arrowEnd, Quaternion.LookRotation(direction), 0.2f, EventType.Repaint);
            }
        }

        private static void DrawGridBounds(Vector2Int gridSize)
        {
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            Vector3 center = new Vector3(gridSize.x / 2f - 0.5f, 0, gridSize.y / 2f - 0.5f);
            Vector3 size = new Vector3(gridSize.x, 0.1f, gridSize.y);

            Handles.DrawWireCube(center, size);
        }

        private static Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x, 0, gridPos.y);
        }

        private static void DrawLegend()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));

            var boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f));

            GUILayout.BeginVertical(boxStyle);

            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 12;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            GUILayout.Label("🗺️ Grid Legend", titleStyle);

            DrawLegendItem("🔵 Cyan", "Warehouse");
            DrawLegendItem("🟢 Green", "Available Cell");
            DrawLegendItem("🔴 Red", "Blocked Cell");
            DrawLegendItem("🟡 Arrow", "Orientation");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void DrawLegendItem(string colorLabel, string description)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 10;
            style.normal.textColor = Color.white;
            GUILayout.Label($"{colorLabel}: {description}", style);
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
