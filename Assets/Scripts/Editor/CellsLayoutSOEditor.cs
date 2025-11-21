using System;
using System.Collections.Generic;
using System.Text;
using Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CellsLayoutSO))]
    public class CellsLayoutSOEditor : UnityEditor.Editor
    {
        private SerializedProperty layoutHashProp;

        private void OnEnable()
        {
            layoutHashProp = serializedObject.FindProperty("layout_hash");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layout Hash", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Current Hash", layoutHashProp.stringValue);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Recalculate Hash"))
            {
                RecalculateHash();
            }

            serializedObject.ApplyModifiedProperties();
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

            Debug.Log($"[CellsLayoutSOEditor] Layout hash recalculated: {newHash}");
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
    }
}