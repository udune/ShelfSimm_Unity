using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class SceneCleanup
    {
        [MenuItem("Tools/ShelfSim/Clean Scene UI 🧹", false, 21)]
        public static void CleanupOldUI()
        {
            if (!EditorUtility.DisplayDialog("Scene UI 정리",
                "기존 UI 오브젝트들을 삭제합니다.\n" +
                "Grid View와 핵심 컴포넌트는 유지됩니다.\n\n" +
                "계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            int deletedCount = 0;

            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                List<Transform> toDelete = new List<Transform>();

                foreach (Transform child in canvas.transform)
                {
                    string name = child.name;

                    if (name.Contains("Grid") ||
                        name.Contains("Robot") ||
                        name.Contains("EmptyCell"))
                    {
                        Debug.Log($"{name} 유지 (Grid View)");
                        continue;
                    }

                    toDelete.Add(child);
                }

                foreach (Transform child in toDelete)
                {
                    Debug.Log($"{child.name} 삭제");
                    Object.DestroyImmediate(child.gameObject);
                    deletedCount++;
                }

                if (canvas.transform.childCount == 0 && canvas.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    Debug.Log("Canvas 전체 삭제 (비어있음)");
                    Object.DestroyImmediate(canvas);
                }
            }

            string[] oldUINames = new string[]
            {
                "Dashboard",
                "CodeInputPanel",
                "InfoPanel",
                "InputPanel",
                "SummaryPanel",
                "CentralPanel",
                "BottomBar",
                "ControlButtons",
                "Legend"
            };

            foreach (string uiName in oldUINames)
            {
                GameObject obj = GameObject.Find(uiName);
                if (obj != null)
                {
                    Debug.Log($"{uiName} 삭제");
                    Object.DestroyImmediate(obj);
                    deletedCount++;
                }
            }

            EditorUtility.SetDirty(SceneManager.GetActiveScene().GetRootGameObjects()[0]);

            Debug.Log($"✅ Scene 정리 완료! {deletedCount}개의 오브젝트 삭제됨");

            EditorUtility.DisplayDialog("완료",
                $"Scene 정리 완료!\n\n" +
                $"삭제된 오브젝트: {deletedCount}개\n" +
                $"Grid View 유지됨 ✅\n\n" +
                "이제 'Tools → ShelfSim → Setup UI (Auto)'를 실행하여\n" +
                "새로운 UI를 생성하세요.",
                "확인");
        }

        [MenuItem("Tools/ShelfSim/Reset Scene (Keep Core) 🔄", false, 22)]
        public static void ResetScene()
        {
            if (!EditorUtility.DisplayDialog("Scene 리셋",
                "Scene을 초기 상태로 리셋합니다.\n" +
                "SimulationManager, Camera, Light, Grid View는 유지됩니다.\n\n" +
                "계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            int deletedCount = 0;

            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.transform.parent != null) continue;

                string name = obj.name;
                if (name == "Main Camera" ||
                    name == "Global Light 2D" ||
                    name.Contains("SimulationManager") ||
                    name.Contains("EventSystem") ||
                    name.Contains("Grid") ||
                    name.Contains("PathFinder") ||
                    name.Contains("PathCache") ||
                    name.Contains("Robot") ||
                    name.Contains("BookRegistry") ||
                    name.Contains("LayoutHashManager"))
                {
                    Debug.Log($"{name} 유지");
                    continue;
                }

                Debug.Log($"{name} 삭제");
                Object.DestroyImmediate(obj);
                deletedCount++;
            }

            Debug.Log($"✅ Scene 리셋 완료! {deletedCount}개의 오브젝트 삭제됨");

            EditorUtility.DisplayDialog("완료",
                $"Scene 리셋 완료!\n\n" +
                $"삭제된 오브젝트: {deletedCount}개\n" +
                $"핵심 컴포넌트 유지: Grid View, SimulationManager 등\n\n" +
                "이제 'Tools → ShelfSim → Setup UI (Auto)'를 실행하여\n" +
                "새로운 UI를 생성하세요.",
                "확인");
        }
    }
}
