using UnityEditor;
using UnityEngine;
using Managers;
using System.Reflection;

namespace Editor
{
    [CustomEditor(typeof(SimulationManager))]
    public class SimulationManagerEditor : UnityEditor.Editor
    {
        private SimulationManager manager;
        private GUIStyle headerStyle;
        private GUIStyle statusStyle;
        private GUIStyle buttonStyle;

        private void OnEnable()
        {
            manager = (SimulationManager)target;
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();

            DrawHeader();
            EditorGUILayout.Space(10);

            if (Application.isPlaying)
            {
                DrawRuntimeControls();
                EditorGUILayout.Space(10);
                DrawRuntimeStatistics();
                EditorGUILayout.Space(10);
            }

            DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
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

            if (statusStyle == null)
            {
                statusStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 11,
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fixedHeight = 30
                };
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🤖 ShelfSim Simulation Manager", headerStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚙️ Runtime Controls", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("▶ Start Test", buttonStyle))
            {
                var jobs = GetTestJobs();
                if (jobs != null && jobs.Count > 0)
                {
                    manager.StartSimulationWithJobs(jobs);
                    Debug.Log($"Started simulation with {jobs.Count} test jobs");
                }
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
            if (GUILayout.Button("⏸ Pause/Resume", buttonStyle))
            {
                manager.TogglePause();
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("⏹ Stop", buttonStyle))
            {
                manager.StopSimulation();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeStatistics()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📊 Runtime Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var summary = manager.GetSummary();
            if (summary != null)
            {
                DrawStatRow("⏱ Elapsed Time", $"{manager.ElapsedTime:F2}s");
                DrawStatRow("✅ Success", summary.success.ToString());
                DrawStatRow("❌ Failures", summary.fail.ToString());
                DrawStatRow("📦 Total Jobs", summary.total.ToString());
                DrawStatRow("⚡ Avg Task Time", $"{manager.AverageTaskTime:F2}s");

                if (summary.fail > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Error Breakdown:", EditorStyles.miniBoldLabel);

                    foreach (var error in summary.error_counts)
                    {
                        if (error.Value > 0)
                        {
                            DrawStatRow($"  • {error.Key}", error.Value.ToString(), true);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No data available", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void DrawStatRow(string label, string value, bool isError = false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));

            var style = new GUIStyle(EditorStyles.label);
            if (isError)
            {
                style.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
            }
            else
            {
                style.normal.textColor = new Color(0.3f, 0.7f, 1f);
            }
            style.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField(value, style);
            EditorGUILayout.EndHorizontal();
        }

        private System.Collections.Generic.List<Data.Job> GetTestJobs()
        {
            var method = typeof(SimulationManager).GetMethod("GetTestJobs",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (method != null)
            {
                return method.Invoke(manager, null) as System.Collections.Generic.List<Data.Job>;
            }

            return null;
        }
    }
}
