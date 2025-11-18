using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Managers;

namespace Editor
{
    /// <summary>
    /// UIManager에 필요한 UI 요소들을 자동으로 생성하는 Editor 스크립트
    /// 사용법: Unity 메뉴 → Tools → Setup UIManager UI Elements
    /// </summary>
    public class UIManagerSetupEditor : EditorWindow
    {
        [MenuItem("Tools/Setup UIManager UI Elements")]
        public static void SetupUIManagerElements()
        {
            // Canvas 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIManagerSetup] Canvas를 찾을 수 없습니다. Scene에 Canvas가 있는지 확인하세요.");
                return;
            }

            // UIManager 찾기
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[UIManagerSetup] UIManager를 찾을 수 없습니다.");
                return;
            }

            Debug.Log("[UIManagerSetup] UIManager UI 요소 생성 시작...");

            // 1. SummaryPanel 생성
            GameObject summaryPanel = CreateSummaryPanel(canvas.transform);
            TextMeshProUGUI summaryText = summaryPanel.GetComponentInChildren<TextMeshProUGUI>();

            // 2. ErrorPanel 생성 (UIManager 전용)
            GameObject errorPanel = CreateErrorPanel(canvas.transform);
            TextMeshProUGUI errorText = errorPanel.GetComponentInChildren<TextMeshProUGUI>();

            // 3. CompletedCountText 생성
            TextMeshProUGUI completedCountText = CreateCompletedCountText(canvas.transform);

            // 4. UIManager에 연결
            SerializedObject so = new SerializedObject(uiManager);

            so.FindProperty("summaryPanel").objectReferenceValue = summaryPanel;
            so.FindProperty("summaryText").objectReferenceValue = summaryText;
            so.FindProperty("completedCountText").objectReferenceValue = completedCountText;
            so.FindProperty("errorPanel").objectReferenceValue = errorPanel;
            so.FindProperty("errorText").objectReferenceValue = errorText;

            // elapsedTimeText와 averageTimeText는 이미 연결되어 있으므로 유지

            so.ApplyModifiedProperties();

            Debug.Log("[UIManagerSetup] ✅ UIManager UI 요소 생성 및 연결 완료!");
            Debug.Log($"  - SummaryPanel: {summaryPanel.name}");
            Debug.Log($"  - ErrorPanel: {errorPanel.name}");
            Debug.Log($"  - CompletedCountText: {completedCountText.name}");

            EditorUtility.SetDirty(uiManager);
            EditorUtility.SetDirty(canvas.gameObject);
        }

        /// <summary>
        /// SummaryPanel 생성 (Panel + ContentPanel + SummaryText + CloseButton)
        /// </summary>
        private static GameObject CreateSummaryPanel(Transform canvasTransform)
        {
            // 기존 SummaryPanel이 있으면 삭제
            Transform existing = canvasTransform.Find("SummaryPanel");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                Debug.Log("[UIManagerSetup] 기존 SummaryPanel 삭제");
            }

            // 1. Background Panel (반투명 검정 배경)
            GameObject summaryPanel = new GameObject("SummaryPanel");
            summaryPanel.transform.SetParent(canvasTransform, false);
            summaryPanel.layer = LayerMask.NameToLayer("UI");

            RectTransform summaryRect = summaryPanel.AddComponent<RectTransform>();
            summaryRect.anchorMin = Vector2.zero;
            summaryRect.anchorMax = Vector2.one;
            summaryRect.sizeDelta = Vector2.zero;
            summaryRect.anchoredPosition = Vector2.zero;

            Image summaryBg = summaryPanel.AddComponent<Image>();
            summaryBg.color = new Color(0, 0, 0, 0.7f); // 반투명 검정

            // 2. Content Panel (흰색 중앙 패널)
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(summaryPanel.transform, false);
            contentPanel.layer = LayerMask.NameToLayer("UI");

            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(800, 600);
            contentRect.anchoredPosition = Vector2.zero;

            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = Color.white;

            // 3. Title Text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(contentPanel.transform, false);
            titleObj.layer = LayerMask.NameToLayer("UI");

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(-40, 50);
            titleRect.anchoredPosition = new Vector2(0, -30);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "시뮬레이션 결과";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.black;

            // 4. Summary Text (스크롤 가능한 텍스트)
            GameObject summaryTextObj = new GameObject("SummaryText");
            summaryTextObj.transform.SetParent(contentPanel.transform, false);
            summaryTextObj.layer = LayerMask.NameToLayer("UI");

            RectTransform summaryTextRect = summaryTextObj.AddComponent<RectTransform>();
            summaryTextRect.anchorMin = new Vector2(0, 0);
            summaryTextRect.anchorMax = new Vector2(1, 1);
            summaryTextRect.sizeDelta = new Vector2(-40, -120); // 상하 여백
            summaryTextRect.anchoredPosition = new Vector2(0, -10);

            TextMeshProUGUI summaryText = summaryTextObj.AddComponent<TextMeshProUGUI>();
            summaryText.text = "";
            summaryText.fontSize = 20;
            summaryText.alignment = TextAlignmentOptions.TopLeft;
            summaryText.color = Color.black;
            summaryText.enableWordWrapping = true;

            // 5. Close Button
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(contentPanel.transform, false);
            closeButtonObj.layer = LayerMask.NameToLayer("UI");

            RectTransform buttonRect = closeButtonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0);
            buttonRect.anchorMax = new Vector2(0.5f, 0);
            buttonRect.sizeDelta = new Vector2(200, 50);
            buttonRect.anchoredPosition = new Vector2(0, 30);

            Image buttonBg = closeButtonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.2f, 0.6f, 1f); // 파란색

            Button button = closeButtonObj.AddComponent<Button>();

            // 버튼 텍스트
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(closeButtonObj.transform, false);
            buttonTextObj.layer = LayerMask.NameToLayer("UI");

            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "닫기";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            // 버튼 이벤트 연결 (UIManager.CloseSummary)
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                button.onClick.AddListener(() => uiManager.CloseSummary());
            }

            // 초기 비활성화
            summaryPanel.SetActive(false);

            return summaryPanel;
        }

        /// <summary>
        /// ErrorPanel 생성 (UIManager 전용)
        /// </summary>
        private static GameObject CreateErrorPanel(Transform canvasTransform)
        {
            // 기존 UIErrorPanel이 있으면 삭제
            Transform existing = canvasTransform.Find("UIErrorPanel");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                Debug.Log("[UIManagerSetup] 기존 UIErrorPanel 삭제");
            }

            // 1. Background Panel (반투명 빨간 배경)
            GameObject errorPanel = new GameObject("UIErrorPanel");
            errorPanel.transform.SetParent(canvasTransform, false);
            errorPanel.layer = LayerMask.NameToLayer("UI");

            RectTransform errorRect = errorPanel.AddComponent<RectTransform>();
            errorRect.anchorMin = Vector2.zero;
            errorRect.anchorMax = Vector2.one;
            errorRect.sizeDelta = Vector2.zero;
            errorRect.anchoredPosition = Vector2.zero;

            Image errorBg = errorPanel.AddComponent<Image>();
            errorBg.color = new Color(0.3f, 0, 0, 0.7f); // 반투명 어두운 빨강

            // 2. Content Panel
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(errorPanel.transform, false);
            contentPanel.layer = LayerMask.NameToLayer("UI");

            RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(600, 400);
            contentRect.anchoredPosition = Vector2.zero;

            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = Color.white;

            // 3. Title Text
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(contentPanel.transform, false);
            titleObj.layer = LayerMask.NameToLayer("UI");

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(-40, 50);
            titleRect.anchoredPosition = new Vector2(0, -30);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "오류 발생";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.8f, 0, 0); // 빨간색

            // 4. Error Text
            GameObject errorTextObj = new GameObject("ErrorText");
            errorTextObj.transform.SetParent(contentPanel.transform, false);
            errorTextObj.layer = LayerMask.NameToLayer("UI");

            RectTransform errorTextRect = errorTextObj.AddComponent<RectTransform>();
            errorTextRect.anchorMin = new Vector2(0, 0);
            errorTextRect.anchorMax = new Vector2(1, 1);
            errorTextRect.sizeDelta = new Vector2(-40, -120);
            errorTextRect.anchoredPosition = new Vector2(0, -10);

            TextMeshProUGUI errorText = errorTextObj.AddComponent<TextMeshProUGUI>();
            errorText.text = "";
            errorText.fontSize = 18;
            errorText.alignment = TextAlignmentOptions.TopLeft;
            errorText.color = Color.black;
            errorText.enableWordWrapping = true;

            // 5. Close Button
            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(contentPanel.transform, false);
            closeButtonObj.layer = LayerMask.NameToLayer("UI");

            RectTransform buttonRect = closeButtonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0);
            buttonRect.anchorMax = new Vector2(0.5f, 0);
            buttonRect.sizeDelta = new Vector2(200, 50);
            buttonRect.anchoredPosition = new Vector2(0, 30);

            Image buttonBg = closeButtonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.8f, 0, 0); // 빨간색

            Button button = closeButtonObj.AddComponent<Button>();

            // 버튼 텍스트
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(closeButtonObj.transform, false);
            buttonTextObj.layer = LayerMask.NameToLayer("UI");

            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "확인";
            buttonText.fontSize = 24;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            // 버튼 이벤트 연결
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                button.onClick.AddListener(() => uiManager.CloseError());
            }

            // 초기 비활성화
            errorPanel.SetActive(false);

            return errorPanel;
        }

        /// <summary>
        /// CompletedCountText 생성 (실시간 대시보드용)
        /// </summary>
        private static TextMeshProUGUI CreateCompletedCountText(Transform canvasTransform)
        {
            // SafeArea 또는 OverlayUI 찾기
            Transform parent = canvasTransform.Find("OverlayUI");
            if (parent == null)
            {
                parent = canvasTransform.Find("SafeArea");
            }
            if (parent == null)
            {
                parent = canvasTransform; // fallback
            }

            // 기존 CompletedCountText가 있으면 삭제
            Transform existing = parent.Find("CompletedCountText");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                Debug.Log("[UIManagerSetup] 기존 CompletedCountText 삭제");
            }

            GameObject textObj = new GameObject("CompletedCountText");
            textObj.transform.SetParent(parent, false);
            textObj.layer = LayerMask.NameToLayer("UI");

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(300, 50);
            rect.anchoredPosition = new Vector2(160, -30);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "완료 건수: 0";
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;

            // 그림자 효과 추가 (가독성 향상)
            text.enableVertexGradient = false;
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;

            return text;
        }
    }
}
