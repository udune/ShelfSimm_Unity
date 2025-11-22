using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Editor
{
    public static class SceneCleanup
    {
        [MenuItem("Tools/ShelfSim/Clean Scene UI", false, 21)]
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
                var toDelete = new List<Transform>();

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

            Debug.Log($"Scene 정리 완료! {deletedCount}개의 오브젝트 삭제됨");

            EditorUtility.DisplayDialog("완료",
                $"Scene 정리 완료!\n\n" +
                $"삭제된 오브젝트: {deletedCount}개\n" +
                $"Grid View 유지됨\n\n" +
                "이제 'Tools → ShelfSim → Setup UI (Auto)'를 실행하여\n" +
                "새로운 UI를 생성하세요.",
                "확인");
        }

        [MenuItem("Tools/ShelfSim/Clean Unnecessary Objects", false, 22)]
        public static void CleanUnnecessaryObjects()
        {
            if (!EditorUtility.DisplayDialog("불필요한 GameObject 정리",
                "다음 GameObject들을 삭제합니다:\n\n" +
                "• Managers (빈 GameObject)\n" +
                "• JobItemPrefab (Scene에 있으면 안 됨)\n\n" +
                "계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            int deletedCount = 0;

            // Managers GameObject 삭제 (빈 컨테이너)
            GameObject managers = GameObject.Find("Managers");
            if (managers != null)
            {
                Debug.Log("Managers 삭제 (빈 GameObject)");
                Object.DestroyImmediate(managers);
                deletedCount++;
            }

            // JobItemPrefab 삭제 (Scene에 있으면 안 됨)
            GameObject jobItemPrefab = GameObject.Find("JobItemPrefab");
            if (jobItemPrefab != null)
            {
                Debug.Log("JobItemPrefab 삭제 (Prefab은 Assets 폴더에 저장해야 함)");
                Object.DestroyImmediate(jobItemPrefab);
                deletedCount++;
            }

            Debug.Log($"정리 완료! {deletedCount}개의 GameObject 삭제됨");

            EditorUtility.DisplayDialog("완료",
                $"불필요한 GameObject 정리 완료!\n\n" +
                $"삭제된 GameObject: {deletedCount}개\n\n" +
                "모든 필수 컴포넌트는 유지되었습니다.",
                "확인");
        }

        [MenuItem("Tools/ShelfSim/Reset Scene (Keep Core)", false, 23)]
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

                // 필수 GameObject 유지
                if (name == "Main Camera" ||
                    name == "Global Light 2D" ||
                    name == "EventSystem" ||
                    name == "SimulationManager" ||
                    name == "BookRegistry" ||
                    name == "LayoutHashManager" ||
                    name == "CellHighlightManager" ||
                    name == "PathCache" ||
                    name == "APIManager" ||
                    name == "CodeManager" ||
                    name == "AStarPathFinder" ||
                    name == "NearestSelector" ||
                    name.Contains("Grid") ||
                    name.Contains("Robot"))
                {
                    Debug.Log($"{name} 유지");
                    continue;
                }

                Debug.Log($"{name} 삭제");
                Object.DestroyImmediate(obj);
                deletedCount++;
            }

            Debug.Log($"Scene 리셋 완료! {deletedCount}개의 오브젝트 삭제됨");

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
