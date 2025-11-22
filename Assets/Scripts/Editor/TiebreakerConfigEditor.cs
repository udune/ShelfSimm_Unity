using UnityEditor;
using UnityEngine;
using Core;

namespace Editor
{
    [CustomEditor(typeof(TiebreakerConfig))]
    public class TiebreakerConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty modeProp;
        private SerializedProperty randomSeedProp;
        private SerializedProperty enableLoggingProp;

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;

        private void OnEnable()
        {
            modeProp = serializedObject.FindProperty("mode");
            randomSeedProp = serializedObject.FindProperty("randomSeed");
            enableLoggingProp = serializedObject.FindProperty("enableLogging");
        }

        public override void OnInspectorGUI()
        {
            InitializeStyles();
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawModeSection();
            EditorGUILayout.Space(10);

            DrawLoggingSection();

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
            EditorGUILayout.LabelField("🎯 Tiebreaker Configuration", headerStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawModeSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("⚖️ Tiebreaker Mode", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(modeProp, new GUIContent("Mode", "How to break ties when multiple cells have same distance"));

            var currentMode = (TiebreakerConfig.TiebreakerMode)modeProp.enumValueIndex;

            if (currentMode == TiebreakerConfig.TiebreakerMode.Alphabetical)
            {
                EditorGUILayout.HelpBox("🔤 Alphabetical: Deterministic, always picks cell with lowest code", MessageType.Info);
            }
            else if (currentMode == TiebreakerConfig.TiebreakerMode.Random)
            {
                EditorGUILayout.HelpBox("🎲 Random: Uses random seed for tie-breaking", MessageType.Info);

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
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLoggingSection()
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.LabelField("📝 Logging", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(enableLoggingProp, new GUIContent("Enable Logging", "Log tiebreaker decisions"));

            if (enableLoggingProp.boolValue)
            {
                EditorGUILayout.HelpBox("⚠️ Logging enabled. May impact performance.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
