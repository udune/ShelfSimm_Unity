using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CellsLayoutSO))]
    public class CellsLayoutSOEditor : UnityEditor.Editor
    {
        private SerializedProperty layoutHashProp;
        private SerializedProperty schemaVersionProp;
        private SerializedProperty gridSizeProp;
        private SerializedProperty warehouseProp;
        private SerializedProperty cellsProp;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private Vector2 cellsScrollPos;

        private void OnEnable()
        {
            layoutHashProp = serializedObject.FindProperty("layout_hash");
            schemaVersionProp = serializedObject.FindProperty("schema_version");
            gridSizeProp = serializedObject.FindProperty("grid_size");
            warehouseProp = serializedObject.FindProperty("warehouse");
            cellsProp = serializedObject.FindProperty("cells");
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawLayoutInfo();
            EditorGUILayout.Space(10);

            DrawHashSection();
            EditorGUILayout.Space(10);

            DrawGridSettings();
            EditorGUILayout.Space(10);

            DrawCellsList();
            EditorGUILayout.Space(10);

            DrawActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.3f, 0.7f, 1f) }
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🗺️ Cells Layout Configuration", headerStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawLayoutInfo()
        {
            CellsLayoutSO layout = target as CellsLayoutSO;
            if (layout == null) return;

            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("📊 Layout Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            int totalCells = layout.cells?.Count ?? 0;
            int blockedCells = layout.cells?.Count(c => c.blocked) ?? 0;
            int availableCells = totalCells - blockedCells;

            DrawStatRow("Total Cells", totalCells.ToString());
            DrawStatRow("Available Cells", availableCells.ToString(), Color.green);
            DrawStatRow("Blocked Cells", blockedCells.ToString(), Color.red);
            DrawStatRow("Grid Size", $"{layout.grid_size.x} × {layout.grid_size.y}");
            DrawStatRow("Warehouse", $"({layout.warehouse.x}, {layout.warehouse.y})");

            EditorGUILayout.EndVertical();
        }

        private void DrawHashSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🔒 Layout Hash", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Current Hash", layoutHashProp.stringValue);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("🔄 Recalculate Hash", GUILayout.Height(25)))
            {
                RecalculateHash();
            }

            EditorGUILayout.HelpBox("Hash is auto-generated from layout data. Recalculate after making changes.", MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawGridSettings()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("⚙️ Grid Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(schemaVersionProp, new GUIContent("Schema Version"));
            EditorGUILayout.PropertyField(gridSizeProp, new GUIContent("Grid Size"));
            EditorGUILayout.PropertyField(warehouseProp, new GUIContent("Warehouse Position"));

            EditorGUILayout.EndVertical();
        }

        private void DrawCellsList()
        {
            EditorGUILayout.BeginVertical(sectionStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("📦 Cells", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                cellsProp.InsertArrayElementAtIndex(cellsProp.arraySize);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            if (cellsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No cells defined. Click '+' to add a cell.", MessageType.Info);
            }
            else
            {
                cellsScrollPos = EditorGUILayout.BeginScrollView(cellsScrollPos, GUILayout.MaxHeight(300));

                for (int i = 0; i < cellsProp.arraySize; i++)
                {
                    DrawCellElement(i);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCellElement(int index)
        {
            SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(index);
            var codeProp = cellProp.FindPropertyRelative("code");
            var xProp = cellProp.FindPropertyRelative("x");
            var yProp = cellProp.FindPropertyRelative("y");
            var blockedProp = cellProp.FindPropertyRelative("blocked");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            string cellLabel = string.IsNullOrEmpty(codeProp.stringValue)
                ? $"Cell {index}"
                : codeProp.stringValue;

            cellProp.isExpanded = EditorGUILayout.Foldout(cellProp.isExpanded, cellLabel, true);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                cellsProp.DeleteArrayElementAtIndex(index);
                return;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (cellProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("code"), new GUIContent("Code"));
                EditorGUILayout.PropertyField(xProp, new GUIContent("X Position"));
                EditorGUILayout.PropertyField(yProp, new GUIContent("Y Position"));
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("width"), new GUIContent("Width"));
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("height"), new GUIContent("Height"));
                EditorGUILayout.PropertyField(cellProp.FindPropertyRelative("orientation"), new GUIContent("Orientation"));
                EditorGUILayout.PropertyField(blockedProp, new GUIContent("Blocked"));

                if (blockedProp.boolValue)
                {
                    EditorGUILayout.HelpBox("⚠️ This cell is blocked", MessageType.Warning);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🛠️ Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔍 Validate Layout", GUILayout.Height(30)))
            {
                ValidateLayout();
            }

            if (GUILayout.Button("📋 Export to JSON", GUILayout.Height(30)))
            {
                ExportToJSON();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatRow(string label, string value, Color? color = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));

            var style = new GUIStyle(EditorStyles.boldLabel);
            if (color.HasValue)
            {
                style.normal.textColor = color.Value;
            }

            EditorGUILayout.LabelField(value, style);
            EditorGUILayout.EndHorizontal();
        }

        private void RecalculateHash()
        {
            CellsLayoutSO layout = target as CellsLayoutSO;
            if (layout == null)
            {
                return;
            }

            string newHash = ComputeLayoutHash(layout);

            layout.layout_hash = newHash;
            EditorUtility.SetDirty(layout);

            Debug.Log($"Layout hash recalculated: {newHash}");
        }

        private string ComputeLayoutHash(CellsLayoutSO layout)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{layout.schema_version}");
            sb.Append($"{layout.grid_size.x},{layout.grid_size.y};");
            sb.Append($"{layout.warehouse.x},{layout.warehouse.y};");

            var sortedCells = new List<CellDef>(layout.cells);
            sortedCells.Sort((a, b) => string.Compare(a.code, b.code, StringComparison.Ordinal));

            foreach (var cell in sortedCells)
            {
                sb.Append($"{cell.code},{cell.x},{cell.y};");
                sb.Append($"{cell.width},{cell.height};");
                sb.Append($"{cell.orientation},{cell.blocked}");
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder result = new StringBuilder();
                for (int i = 0; i < 8; i++)
                {
                    result.Append(hash[i].ToString("x2"));
                }

                return $"sha256:{result}";
            }
        }

        private void ValidateLayout()
        {
            CellsLayoutSO layout = target as CellsLayoutSO;
            if (layout == null) return;

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            // Check grid size
            if (layout.grid_size.x <= 0 || layout.grid_size.y <= 0)
            {
                errors.Add("Grid size must be positive");
            }

            // Check cells
            HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
            HashSet<string> codes = new HashSet<string>();

            foreach (var cell in layout.cells)
            {
                if (string.IsNullOrEmpty(cell.code))
                {
                    errors.Add($"Cell at ({cell.x}, {cell.y}) has no code");
                }
                else if (codes.Contains(cell.code))
                {
                    errors.Add($"Duplicate cell code: {cell.code}");
                }
                else
                {
                    codes.Add(cell.code);
                }

                var pos = new Vector2Int(cell.x, cell.y);
                if (positions.Contains(pos))
                {
                    errors.Add($"Duplicate cell position: ({cell.x}, {cell.y})");
                }
                else
                {
                    positions.Add(pos);
                }

                if (cell.x < 0 || cell.x >= layout.grid_size.x || cell.y < 0 || cell.y >= layout.grid_size.y)
                {
                    errors.Add($"Cell {cell.code} is outside grid bounds");
                }

                if (cell.blocked)
                {
                    warnings.Add($"Cell {cell.code} is blocked");
                }
            }

            // Display results
            string result = "=== Layout Validation ===\n\n";

            if (errors.Count == 0)
            {
                result += "✅ No errors found!\n\n";
            }
            else
            {
                result += $"❌ {errors.Count} error(s) found:\n";
                foreach (var error in errors)
                {
                    result += $"  • {error}\n";
                }
                result += "\n";
            }

            if (warnings.Count > 0)
            {
                result += $"⚠️ {warnings.Count} warning(s):\n";
                foreach (var warning in warnings)
                {
                    result += $"  • {warning}\n";
                }
            }

            Debug.Log(result);
            EditorUtility.DisplayDialog("Validation Results", result, "OK");
        }

        private void ExportToJSON()
        {
            CellsLayoutSO layout = target as CellsLayoutSO;
            if (layout == null) return;

            string json = JsonUtility.ToJson(layout, true);
            string path = EditorUtility.SaveFilePanel("Export Layout to JSON", "", "layout.json", "json");

            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"Layout exported to: {path}");
                EditorUtility.DisplayDialog("Export Successful", $"Layout exported to:\n{path}", "OK");
            }
        }
    }
}
