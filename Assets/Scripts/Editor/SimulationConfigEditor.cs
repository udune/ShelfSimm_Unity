using UnityEditor;
using UnityEngine;
using Core;

namespace Editor
{
    [CustomEditor(typeof(SimulationConfig))]
    public class SimulationConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty handleTimeProp;
        private SerializedProperty robotSpeedProp;
        private SerializedProperty moveTimeoutProp;
        private SerializedProperty topNProp;
        private SerializedProperty randomSeedProp;
        private SerializedProperty warehousePosProp;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;

        private void OnEnable()
        {
            handleTimeProp = serializedObject.FindProperty("_handleTime");
            robotSpeedProp = serializedObject.FindProperty("robotSpeed");
            moveTimeoutProp = serializedObject.FindProperty("moveTimeoutSec");
            topNProp = serializedObject.FindProperty("topN");
            randomSeedProp = serializedObject.FindProperty("randomSeed");
            warehousePosProp = serializedObject.FindProperty("warehousePos");
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawTaskSection();
            EditorGUILayout.Space(10);

            DrawRobotSection();
            EditorGUILayout.Space(10);

            DrawPathfindingSection();
            EditorGUILayout.Space(10);

            DrawDeterminismSection();
            EditorGUILayout.Space(10);

            DrawWarehouseSection();

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
            EditorGUILayout.LabelField("⚙️ Simulation Configuration", headerStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawTaskSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("📦 Task Processing", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(handleTimeProp, new GUIContent("Handle Time (s)", "Time to handle a book at a cell"));

            var config = target as SimulationConfig;
            if (config != null && Application.isPlaying)
            {
                EditorGUILayout.HelpBox($"Current: {config.handleTime:F2}s", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRobotSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🤖 Robot Movement", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(robotSpeedProp, new GUIContent("Robot Speed", "Movement speed in units/second"));
            EditorGUILayout.PropertyField(moveTimeoutProp, new GUIContent("Move Timeout (s)", "Max time allowed for movement before timeout"));

            EditorGUILayout.EndVertical();
        }

        private void DrawPathfindingSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🗺️ Pathfinding", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(topNProp, new GUIContent("Top N Candidates", "Number of cells to re-evaluate with A*"));
            EditorGUILayout.HelpBox("Higher values = more accurate, slower. Recommended: 3-5", MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        private void DrawDeterminismSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🎲 Determinism", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(randomSeedProp, new GUIContent("Random Seed", "Seed for deterministic randomness"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🎲 Randomize"))
            {
                randomSeedProp.intValue = Random.Range(0, 100000);
            }
            if (GUILayout.Button("↺ Reset to 42"))
            {
                randomSeedProp.intValue = 42;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawWarehouseSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("🏭 Warehouse", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(warehousePosProp, new GUIContent("Warehouse Position", "Starting position for robots"));

            EditorGUILayout.EndVertical();
        }
    }
}
