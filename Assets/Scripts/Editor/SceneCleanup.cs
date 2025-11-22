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
                "기존 UI 오브젝트들을 모두 삭제합니다.\n" +
                "SimulationManager와 핵심 컴포넌트는 유지됩니다.\n\n" +
                "계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            int deletedCount = 0;

            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Debug.Log("Canvas 및 모든 UI 삭제 중...");
                Object.DestroyImmediate(canvas);
                deletedCount++;
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
                "GridContainer",
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
                $"삭제된 오브젝트: {deletedCount}개\n\n" +
                "이제 'Tools → ShelfSim → Setup UI (Auto)'를 실행하여\n" +
                "새로운 UI를 생성하세요.",
                "확인");
        }

        [MenuItem("Tools/ShelfSim/Reset Scene (Keep Core) 🔄", false, 22)]
        public static void ResetScene()
        {
            if (!EditorUtility.DisplayDialog("Scene 리셋",
                "Scene을 초기 상태로 리셋합니다.\n" +
                "SimulationManager, Camera, Light만 남기고 모두 삭제됩니다.\n\n" +
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
                    name.Contains("EventSystem"))
                {
                    continue;
                }

                Debug.Log($"{name} 삭제");
                Object.DestroyImmediate(obj);
                deletedCount++;
            }

            Debug.Log($"✅ Scene 리셋 완료! {deletedCount}개의 오브젝트 삭제됨");

            EditorUtility.DisplayDialog("완료",
                $"Scene 리셋 완료!\n\n" +
                $"삭제된 오브젝트: {deletedCount}개\n\n" +
                "이제 'Tools → ShelfSim → Setup UI (Auto)'를 실행하여\n" +
                "새로운 UI를 생성하세요.",
                "확인");
        }
    }
}
