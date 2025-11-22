using UnityEditor;
using UnityEngine;
using Managers;
using System.Linq;
using System.Reflection;

namespace Editor
{
    public class SimulationControlWindow : EditorWindow
    {
        private SimulationManager manager;
        private Vector2 scrollPosition;
        private bool autoRefresh = true;
        private double lastUpdateTime;
        private const double UPDATE_INTERVAL = 0.1;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle statLabelStyle;
        private GUIStyle statValueStyle;

        [MenuItem("Window/ShelfSim/Simulation Control")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimulationControlWindow>("Simulation Control");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            FindManager();
        }

        private void OnGUI()
        {
            InitializeStyles();

            if (manager == null)
            {
                FindManager();
                if (manager == null)
                {
                    DrawNoManagerWarning();
                    return;
                }
            }

            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawStatusPanel();
            EditorGUILayout.Space(10);

            DrawControlPanel();
            EditorGUILayout.Space(10);

            DrawStatisticsPanel();
            EditorGUILayout.Space(10);

            if (Application.isPlaying)
            {
                DrawRobotsPanel();
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();

            if (Application.isPlaying && autoRefresh)
            {
                if (EditorApplication.timeSinceStartup - lastUpdateTime > UPDATE_INTERVAL)
                {
                    Repaint();
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                }
            }
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.2f, 0.6f, 1f) }
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }

            if (statLabelStyle == null)
            {
                statLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11
                };
            }

            if (statValueStyle == null)
            {
                statValueStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 11,
                    normal = { textColor = new Color(0.3f, 0.7f, 1f) }
                };
            }
        }

        private void FindManager()
        {
            manager = FindObjectOfType<SimulationManager>();
        }

        private void DrawNoManagerWarning()
        {
            EditorGUILayout.HelpBox(
                "⚠️ SimulationManager not found in scene!\n\nPlease add SimulationManager to your scene.",
                MessageType.Warning
            );

            if (GUILayout.Button("Refresh"))
            {
                FindManager();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("🤖 ShelfSim Control Panel", headerStyle);

            GUILayout.FlexibleSpace();

            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));

            if (GUILayout.Button("🔄", EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                FindManager();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusPanel()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("📡 Status", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (!Application.isPlaying)
            {
                DrawStatusRow("State", "⚫ Editor Mode", Color.gray);
            }
            else
            {
                bool isRunning = GetPrivateField<bool>("_isRunning");
                bool isPaused = GetPrivateField<bool>("_isPaused");

                string status;
                Color color;

                if (isRunning && !isPaused)
                {
                    status = "🟢 Running";
                    color = Color.green;
                }
                else if (isRunning && isPaused)
                {
                    status = "🟡 Paused";
                    color = Color.yellow;
                }
                else
                {
                    status = "🔴 Stopped";
                    color = Color.red;
                }

                DrawStatusRow("State", status, color);
                DrawStatusRow("Elapsed", $"{manager.ElapsedTime:F2}s", Color.cyan);

                bool useApiMode = GetSerializedField<bool>("useApiMode");
                DrawStatusRow("Mode", useApiMode ? "🌐 API Mode" : "💻 Local Mode", Color.white);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawControlPanel()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("⚙️ Controls", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("▶ Start", GUILayout.Height(35)))
            {
                var jobs = GetTestJobs();
                if (jobs != null)
                {
                    manager.StartSimulationWithJobs(jobs);
                }
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
            if (GUILayout.Button("⏸ Pause/Resume", GUILayout.Height(35)))
            {
                manager.TogglePause();
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("⏹ Stop", GUILayout.Height(35)))
            {
                manager.StopSimulation();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatisticsPanel()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("📊 Statistics", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var summary = manager.GetSummary();
            if (summary == null)
            {
                EditorGUILayout.LabelField("No data available", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawStatRow("Total Jobs", summary.total.ToString());
                DrawStatRow("✅ Success", summary.success.ToString(), new Color(0.3f, 0.8f, 0.3f));
                DrawStatRow("❌ Failed", summary.fail.ToString(), new Color(0.9f, 0.3f, 0.3f));

                if (summary.success > 0)
                {
                    float successRate = (float)summary.success / summary.total * 100f;
                    DrawStatRow("Success Rate", $"{successRate:F1}%", Color.cyan);
                }

                DrawStatRow("Avg Task Time", $"{manager.AverageTaskTime:F2}s", Color.yellow);

                if (summary.fail > 0 && summary.reasons != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Error Breakdown:", EditorStyles.miniBoldLabel);

                    foreach (var error in summary.reasons.OrderByDescending(x => x.Value))
                    {
                        if (error.Value > 0)
                        {
                            DrawStatRow($"  {error.Key}", error.Value.ToString(), new Color(1f, 0.5f, 0.3f));
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRobotsPanel()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🤖 Robot Status", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var robotController = GetSerializedField<Core.RobotController>("robotController");
            if (robotController != null)
            {
                var robots = GetRobotControllerField<System.Collections.Generic.List<Data.RobotData>>(robotController, "robots");

                if (robots != null && robots.Count > 0)
                {
                    foreach (var robot in robots)
                    {
                        DrawRobotInfo(robot);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No robots available", EditorStyles.centeredGreyMiniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("RobotController not found", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRobotInfo(Data.RobotData robot)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Robot {robot.id}", EditorStyles.boldLabel, GUILayout.Width(80));

            string stateIcon = robot.state switch
            {
                Data.RobotState.IDLE => "⚪",
                Data.RobotState.MOVING => "🔵",
                Data.RobotState.HANDLING => "🟠",
                Data.RobotState.RETURNING => "🟡",
                _ => "⚫"
            };

            EditorGUILayout.LabelField($"{stateIcon} {robot.state}", GUILayout.Width(100));

            if (!string.IsNullOrEmpty(robot.targetCode))
            {
                EditorGUILayout.LabelField($"Target: {robot.targetCode}", GUILayout.Width(120));
            }

            EditorGUILayout.EndHorizontal();

            if (robot.errorCode != Data.ErrorCode.INVALID_VALUE)
            {
                var errorStyle = new GUIStyle(EditorStyles.miniLabel);
                errorStyle.normal.textColor = Color.red;
                EditorGUILayout.LabelField($"⚠ Error: {robot.errorCode}", errorStyle);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusRow(string label, string value, Color valueColor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, statLabelStyle, GUILayout.Width(120));

            var style = new GUIStyle(statValueStyle);
            style.normal.textColor = valueColor;
            EditorGUILayout.LabelField(value, style);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatRow(string label, string value, Color? valueColor = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(150));

            var style = new GUIStyle(EditorStyles.boldLabel);
            if (valueColor.HasValue)
            {
                style.normal.textColor = valueColor.Value;
            }

            EditorGUILayout.LabelField(value, style);
            EditorGUILayout.EndHorizontal();
        }

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(SimulationManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return (T)field.GetValue(manager);
            }
            return default(T);
        }

        private T GetSerializedField<T>(string fieldName)
        {
            var field = typeof(SimulationManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                return (T)field.GetValue(manager);
            }
            return default(T);
        }

        private T GetRobotControllerField<T>(Core.RobotController controller, string fieldName)
        {
            var field = typeof(Core.RobotController).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                return (T)field.GetValue(controller);
            }
            return default(T);
        }

        private System.Collections.Generic.List<Data.Job> GetTestJobs()
        {
            var method = typeof(SimulationManager).GetMethod("GetTestJobs", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                return method.Invoke(manager, null) as System.Collections.Generic.List<Data.Job>;
            }
            return null;
        }
    }
}
