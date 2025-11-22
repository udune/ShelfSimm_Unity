using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ManagerSetup
    {
        [MenuItem("Tools/ShelfSim/Setup Managers", false, 10)]
        public static void SetupManagers()
        {
            if (!EditorUtility.DisplayDialog("Manager GameObject 설정",
                "Managers GameObject와 하위 Manager 컴포넌트들을 생성/확인합니다.\n\n" +
                "계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            // Managers 부모 GameObject 찾기 또는 생성
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
                Debug.Log("Managers GameObject 생성");
            }

            // SimulationManager
            CreateOrFindManager<Managers.Managers.SimulationManager>(managers.transform, "SimulationManager");

            // BookRegistry
            CreateOrFindManager<Core.Core.BookRegistry>(managers.transform, "BookRegistry");

            // LayoutHashManager
            CreateOrFindManager<Managers.Managers.LayoutHashManager>(managers.transform, "LayoutHashManager");

            // CellHighlightManager
            CreateOrFindManager<Managers.Managers.CellHighlightManager>(managers.transform, "CellHighlightManager");

            // PathCache
            CreateOrFindManager<Core.Core.PathCache>(managers.transform, "PathCache");

            // APIManager (ApiClient)
            CreateOrFindManager<API.API.ApiClient>(managers.transform, "APIManager");

            // CodeManager - CodeValidator + CodeRegistry
            GameObject codeManager = FindOrCreateGameObject(managers.transform, "CodeManager");
            if (codeManager.GetComponent<Core.Core.CodeValidator>() == null)
            {
                codeManager.AddComponent<Core.Core.CodeValidator>();
                Debug.Log("CodeValidator 컴포넌트 추가");
            }
            if (codeManager.GetComponent<Core.Core.CodeRegistry>() == null)
            {
                codeManager.AddComponent<Core.Core.CodeRegistry>();
                Debug.Log("CodeRegistry 컴포넌트 추가");
            }

            EditorUtility.DisplayDialog("완료",
                "Manager GameObject 설정 완료!\n\n" +
                "Managers GameObject 아래에 모든 Manager들이 생성/확인되었습니다.",
                "확인");

            Debug.Log("Manager 설정 완료!");
        }

        private static void CreateOrFindManager<T>(Transform parent, string name) where T : Component
        {
            GameObject obj = FindOrCreateGameObject(parent, name);

            if (obj.GetComponent<T>() == null)
            {
                obj.AddComponent<T>();
                Debug.Log($"{name}에 {typeof(T).Name} 컴포넌트 추가");
            }
            else
            {
                Debug.Log($"{name} 이미 존재함");
            }
        }

        private static GameObject FindOrCreateGameObject(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Debug.Log($"{name} GameObject 생성");
            return obj;
        }
    }
}
