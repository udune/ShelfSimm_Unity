using System.Collections.Generic;
using System.Reflection;
using API;
using UnityEditor;
using UnityEngine;
using Managers;
using Core;
using Data;

namespace Editor
{
    public static class ShelfSimMenuItems
    {
        [MenuItem("Window/ShelfSim/Control Panel %#C")]
        public static void OpenControlPanel()
        {
            SimulationControlWindow.ShowWindow();
        }

        [MenuItem("Window/ShelfSim/Toggle Grid Visualization %#G")]
        public static void ToggleGridVisualization()
        {
            // This calls the static method in GridVisualizationEditor
            var method = typeof(GridVisualizationEditor).GetMethod("ToggleVisualization",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, null);
        }

        [MenuItem("GameObject/ShelfSim/Create Simulation Manager", false, 10)]
        public static void CreateSimulationManager()
        {
            var go = new GameObject("SimulationManager");
            go.AddComponent<SimulationManager>();
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("Created SimulationManager");
        }

        [MenuItem("Assets/Create/ShelfSim/Simulation Config", false, 100)]
        public static void CreateSimulationConfig()
        {
            CreateScriptableObjectAsset<SimulationConfig>("SimulationConfig");
        }

        [MenuItem("Assets/Create/ShelfSim/Tiebreaker Config", false, 101)]
        public static void CreateTiebreakerConfig()
        {
            CreateScriptableObjectAsset<TiebreakerConfig>("TiebreakerConfig");
        }

        [MenuItem("Assets/Create/ShelfSim/Cells Layout", false, 102)]
        public static void CreateCellsLayout()
        {
            CreateScriptableObjectAsset<CellsLayoutSO>("CellsLayout");
        }

        [MenuItem("Tools/ShelfSim/Find Simulation Manager")]
        public static void FindSimulationManager()
        {
            var manager = Object.FindObjectOfType<SimulationManager>();
            if (manager != null)
            {
                Selection.activeGameObject = manager.gameObject;
                EditorGUIUtility.PingObject(manager.gameObject);
                Debug.Log("Found SimulationManager");
            }
            else
            {
                Debug.LogWarning("No SimulationManager found in scene!");
                if (EditorUtility.DisplayDialog("SimulationManager Not Found",
                    "No SimulationManager found in the current scene. Would you like to create one?",
                    "Yes", "No"))
                {
                    CreateSimulationManager();
                }
            }
        }

        [MenuItem("Tools/ShelfSim/Validate Setup")]
        public static void ValidateSetup()
        {
            bool isValid = true;
            string report = "=== ShelfSim Setup Validation ===\n\n";

            // Check SimulationManager
            var manager = Object.FindObjectOfType<SimulationManager>();
            if (manager != null)
            {
                report += "✅ SimulationManager: Found\n";

                // Check required components
                var config = GetPrivateField<SimulationConfig>(manager, "config");
                var apiClient = GetPrivateField<ApiClient>(manager, "apiClient");
                var robotController = GetPrivateField<RobotController>(manager, "robotController");
                var pathFinder = GetPrivateField<SimpleAStarPathFinder>(manager, "pathFinder");
                var cellsLayout = GetPrivateField<CellsLayoutSO>(manager, "cellsLayout");

                report += $"{(config != null ? "✅" : "❌")} SimulationConfig: {(config != null ? "Set" : "Missing")}\n";
                report += $"{(robotController != null ? "✅" : "❌")} RobotController: {(robotController != null ? "Set" : "Missing")}\n";
                report += $"{(pathFinder != null ? "✅" : "❌")} PathFinder: {(pathFinder != null ? "Set" : "Missing")}\n";
                report += $"{(cellsLayout != null ? "✅" : "❌")} CellsLayout: {(cellsLayout != null ? "Set" : "Missing")}\n";
                report += $"{(apiClient != null ? "✅" : "❌")} ApiClient: {(apiClient != null ? "Set" : "Optional")}\n";

                if (config == null || robotController == null || pathFinder == null || cellsLayout == null)
                {
                    isValid = false;
                }
            }
            else
            {
                report += "❌ SimulationManager: Not Found\n";
                isValid = false;
            }

            report += "\n" + (isValid ? "✅ Setup is valid!" : "❌ Setup has issues. Please fix missing components.");

            Debug.Log(report);

            EditorUtility.DisplayDialog("Setup Validation",
                report,
                "OK");
        }

        [MenuItem("Tools/ShelfSim/Clear Player Prefs")]
        public static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear Player Prefs",
                "Are you sure you want to clear all PlayerPrefs?",
                "Yes", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("PlayerPrefs cleared");
            }
        }

        [MenuItem("Tools/ShelfSim/Documentation/Open GitHub")]
        public static void OpenGitHub()
        {
            Application.OpenURL("https://github.com/udune/ShelfSimm_Unity");
        }

        private static void CreateScriptableObjectAsset<T>(string defaultName) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (System.IO.Path.GetExtension(path) != "")
            {
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + defaultName + ".asset");

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            Debug.Log($"Created {typeof(T).Name} at {assetPathAndName}");
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                return (T)field.GetValue(obj);
            }
            return default(T);
        }
    }
}
