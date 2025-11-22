using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UI;
using Managers;
using Core;

namespace Editor
{
    public static class SimulationUISetup
    {
        private static TMP_FontAsset boldFont;
        private static TMP_FontAsset regularFont;

        [MenuItem("Tools/ShelfSim/Setup UI (Auto) ⚡", false, 20)]
        public static void SetupCompleteUI()
        {
            if (!EditorUtility.DisplayDialog("UI 자동 설정",
                "현재 Scene에 완전한 시뮬레이션 UI를 자동으로 생성합니다.\n" +
                "기존 UI가 있다면 덮어쓰여질 수 있습니다.\n\n계속하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            LoadFonts();

            Canvas canvas = FindOrCreateCanvas();

            GameObject gridViewArea = CreateGridViewArea(canvas);
            GameObject controlArea = CreateControlArea(canvas);

            GameObject jobInputPanel = CreateJobInputPanel(controlArea.transform);
            GameObject jobListPanel = CreateJobListPanel(controlArea.transform);
            GameObject statusPanel = CreateStatusPanel(controlArea.transform);

            GameObject gridInfoPanel = CreateGridInfoPanel(gridViewArea.transform);
            GameObject gridControlPanel = CreateGridControlPanel(gridViewArea.transform);

            JobInputController jobInputController = jobInputPanel.GetComponent<JobInputController>();
            SimulationUIController uiController = jobListPanel.GetComponent<SimulationUIController>();

            ConnectReferences(jobInputController, uiController);

            Selection.activeGameObject = canvas.gameObject;
            EditorGUIUtility.PingObject(canvas.gameObject);

            Debug.Log("UI 자동 설정 완료!");
            EditorUtility.DisplayDialog("완료",
                "시뮬레이션 UI가 성공적으로 생성되었습니다!\n\n" +
                "좌측 60%: Grid View (로봇 시뮬레이션)\n" +
                "우측 40%: 작업 입력/목록/상태\n\n" +
                "- CellInfoPanel: 셀 정보 표시\n" +
                "- GridControlPanel: 시뮬레이션 제어\n" +
                "- JobInputController: 작업 입력\n" +
                "- SimulationUIController: 작업 목록 및 실행\n\n" +
                "모든 참조가 자동 연결되었습니다.",
                "확인");
        }

        private static Canvas FindOrCreateCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                scaler.referencePixelsPerUnit = 100;

                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("Canvas 생성됨 (1920x1080 반응형)");
            }
            else
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    scaler.referencePixelsPerUnit = 100;
                    Debug.Log("기존 Canvas를 1920x1080 반응형으로 설정");
                }
            }
            return canvas;
        }

        private static void LoadFonts()
        {
            string[] fontGuids = AssetDatabase.FindAssets("NotoSansKR-Bold t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                Debug.Log($"Bold 폰트 로드 성공: {path}");
            }
            else
            {
                Debug.LogWarning("NotoSansKR-Bold 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.");
            }

            fontGuids = AssetDatabase.FindAssets("Pretendard-Regular t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                Debug.Log($"Regular 폰트 로드 성공: {path}");
            }
            else
            {
                Debug.LogWarning("Pretendard-Regular 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.");
            }
        }

        private static GameObject CreateGridViewArea(Canvas canvas)
        {
            GameObject area = new GameObject("GridViewArea");
            area.transform.SetParent(canvas.transform, false);

            RectTransform rect = area.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.6f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return area;
        }

        private static GameObject CreateControlArea(Canvas canvas)
        {
            GameObject area = new GameObject("ControlArea");
            area.transform.SetParent(canvas.transform, false);

            RectTransform rect = area.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.6f, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = area.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            UnityEngine.UI.Shadow shadow = area.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(4, -4);

            return area;
        }

        private static GameObject CreateJobInputPanel(Transform parent)
        {
            GameObject panel = new GameObject("JobInputPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.67f);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.20f, 1f);

            UnityEngine.UI.Shadow shadow = panel.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            GameObject title = CreateText(panel.transform, "Title", "작업 입력", 24, TextAlignmentOptions.Center, true);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.92f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            float yStart = 0.83f;
            float spacing = 0.15f;

            GameObject cellCodesInput = CreateInputField(panel.transform, "CellCodesInput", "셀 코드 (예: A01, A02-A05)", yStart);
            yStart -= spacing;

            GameObject actionDropdown = CreateDropdown(panel.transform, "ActionDropdown", new string[] { "PUT", "PICK" }, yStart);
            CreateLabel(panel.transform, "ActionLabel", "작업 유형", yStart + 0.05f);
            yStart -= spacing;

            GameObject bookDropdown = CreateDropdown(panel.transform, "BookDropdown", new string[] { "책 선택" }, yStart);
            CreateLabel(panel.transform, "BookLabel", "도서 선택", yStart + 0.05f);
            yStart -= spacing;

            GameObject quantityInput = CreateInputField(panel.transform, "QuantityInput", "수량", yStart);
            CreateLabel(panel.transform, "QuantityLabel", "수량", yStart + 0.05f);
            yStart -= spacing + 0.05f;

            GameObject addButton = CreateButton(panel.transform, "AddJobButton", "작업 추가", yStart, new Color(0.25f, 0.75f, 0.35f));

            GameObject errorPanel = CreateErrorPanel(panel.transform);

            JobInputController controller = panel.AddComponent<JobInputController>();

            var cellInput = cellCodesInput.GetComponent<TMP_InputField>();
            var actionDrop = actionDropdown.GetComponent<TMP_Dropdown>();
            var bookDrop = bookDropdown.GetComponent<TMP_Dropdown>();
            var quantityInputField = quantityInput.GetComponent<TMP_InputField>();
            var executeBtn = addButton.GetComponent<Button>();

            typeof(JobInputController).GetField("cellCodesInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, cellInput);
            typeof(JobInputController).GetField("actionTypeDropdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, actionDrop);
            typeof(JobInputController).GetField("bookDropdown",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, bookDrop);
            typeof(JobInputController).GetField("quantityInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, quantityInputField);
            typeof(JobInputController).GetField("executeButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, executeBtn);
            typeof(JobInputController).GetField("errorPanel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, errorPanel);

            var bookRegistry = Object.FindObjectOfType<BookRegistry>();
            if (bookRegistry != null)
            {
                typeof(JobInputController).GetField("bookRegistry",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(controller, bookRegistry);
            }

            EditorUtility.SetDirty(controller);

            return panel;
        }

        private static GameObject CreateJobListPanel(Transform parent)
        {
            GameObject panel = new GameObject("JobListPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.33f);
            rect.anchorMax = new Vector2(1, 0.66f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.20f, 1f);

            UnityEngine.UI.Shadow shadow = panel.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            GameObject jobCountText = CreateText(panel.transform, "JobCountText", "작업 목록 (0개)", 24, TextAlignmentOptions.Left, true);
            RectTransform countRect = jobCountText.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0, 0.9f);
            countRect.anchorMax = new Vector2(0.7f, 1);
            countRect.offsetMin = new Vector2(10, 0);
            countRect.offsetMax = new Vector2(-10, -10);

            GameObject scrollView = CreateScrollView(panel.transform, "JobListScrollView");
            RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.2f);
            scrollRect.anchorMax = new Vector2(1, 0.88f);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);

            Transform content = scrollView.transform.Find("Viewport/Content");

            GameObject clearButton = CreateButton(panel.transform, "ClearAllButton", "전체 삭제", 0.12f, new Color(0.85f, 0.25f, 0.25f));
            RectTransform clearRect = clearButton.GetComponent<RectTransform>();
            clearRect.anchorMin = new Vector2(0, 0.05f);
            clearRect.anchorMax = new Vector2(0.48f, 0.15f);
            clearRect.offsetMin = new Vector2(10, 10);
            clearRect.offsetMax = new Vector2(-5, -10);

            GameObject startButton = CreateButton(panel.transform, "StartSimulationButton", "시뮬레이션 시작", 0.12f, new Color(0.2f, 0.6f, 1.0f));
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.52f, 0.05f);
            startRect.anchorMax = new Vector2(1, 0.15f);
            startRect.offsetMin = new Vector2(5, 10);
            startRect.offsetMax = new Vector2(-10, -10);

            GameObject jobItemPrefab = CreateJobItemPrefab();

            SimulationUIController controller = panel.AddComponent<SimulationUIController>();

            var simManager = Object.FindObjectOfType<SimulationManager>();
            var bookRegistry = Object.FindObjectOfType<BookRegistry>();

            typeof(SimulationUIController).GetField("simulationManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, simManager);
            typeof(SimulationUIController).GetField("bookRegistry",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, bookRegistry);
            typeof(SimulationUIController).GetField("jobListContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, content);
            typeof(SimulationUIController).GetField("jobItemPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, jobItemPrefab);
            typeof(SimulationUIController).GetField("jobCountText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, jobCountText.GetComponent<TextMeshProUGUI>());
            typeof(SimulationUIController).GetField("clearAllButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, clearButton.GetComponent<Button>());
            typeof(SimulationUIController).GetField("startSimulationButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, startButton.GetComponent<Button>());

            EditorUtility.SetDirty(controller);

            return panel;
        }

        private static GameObject CreateStatusPanel(Transform parent)
        {
            GameObject panel = new GameObject("StatusPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.32f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.16f, 1f);

            UnityEngine.UI.Shadow shadow = panel.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            GameObject titleObj = CreateText(panel.transform, "Title", "시뮬레이션 상태", 20, TextAlignmentOptions.Left, true);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.95f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, -5);

            GameObject statusText = CreateText(panel.transform, "StatusText", "", 15, TextAlignmentOptions.TopLeft);
            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0.92f);
            statusRect.offsetMin = new Vector2(20, 15);
            statusRect.offsetMax = new Vector2(-20, -5);

            return panel;
        }

        private static void ConnectReferences(JobInputController jobInputController, SimulationUIController uiController)
        {
            typeof(SimulationUIController).GetField("jobInputController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(uiController, jobInputController);

            EditorUtility.SetDirty(uiController);
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment, bool isBold = false)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            if (isBold && boldFont != null)
            {
                tmp.font = boldFont;
            }
            else if (!isBold && regularFont != null)
            {
                tmp.font = regularFont;
            }

            return obj;
        }

        private static GameObject CreateLabel(Transform parent, string name, string text, float yPos)
        {
            GameObject label = CreateText(parent, name, text, 14, TextAlignmentOptions.Left);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, yPos);
            rect.anchorMax = new Vector2(0.95f, yPos + 0.04f);
            return label;
        }

        private static GameObject CreateInputField(Transform parent, string name, string placeholder, float yPos)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, yPos - 0.08f);
            rect.anchorMax = new Vector2(0.95f, yPos);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            UnityEngine.UI.Outline outline = obj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.4f, 0.4f, 0.5f, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(obj.transform, false);
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = new Vector2(0.02f, 0.1f);
            textAreaRect.anchorMax = new Vector2(0.98f, 0.9f);
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 14;
            if (regularFont != null) textComponent.font = regularFont;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI placeholderComponent = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderComponent.text = placeholder;
            placeholderComponent.fontSize = 14;
            placeholderComponent.fontStyle = FontStyles.Italic;
            placeholderComponent.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            if (regularFont != null) placeholderComponent.font = regularFont;
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;

            TMP_InputField inputField = obj.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholderComponent;

            return obj;
        }

        private static GameObject CreateDropdown(Transform parent, string name, string[] options, float yPos)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, yPos - 0.08f);
            rect.anchorMax = new Vector2(0.95f, yPos);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            UnityEngine.UI.Outline outline = obj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.4f, 0.4f, 0.5f, 0.5f);
            outline.effectDistance = new Vector2(1, -1);

            GameObject label = new GameObject("Label");
            label.transform.SetParent(obj.transform, false);
            TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.text = options[0];
            labelText.fontSize = 14;
            if (regularFont != null) labelText.font = regularFont;
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.02f, 0.1f);
            labelRect.anchorMax = new Vector2(0.9f, 0.9f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            GameObject arrow = new GameObject("Arrow");
            arrow.transform.SetParent(obj.transform, false);
            TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 12;
            arrowText.alignment = TextAlignmentOptions.Center;
            if (regularFont != null) arrowText.font = regularFont;
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0);
            arrowRect.anchorMax = new Vector2(1, 1);
            arrowRect.sizeDelta = new Vector2(20, 0);
            arrowRect.anchoredPosition = new Vector2(-10, 0);

            GameObject template = new GameObject("Template");
            template.transform.SetParent(obj.transform, false);
            RectTransform templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.sizeDelta = new Vector2(0, 150);
            templateRect.anchoredPosition = new Vector2(0, 0);

            Image templateBg = template.AddComponent<Image>();
            templateBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 28);

            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            RectTransform itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 20);

            Image itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            GameObject itemLabel = new GameObject("ItemLabel");
            itemLabel.transform.SetParent(item.transform, false);
            TextMeshProUGUI itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemLabelText.fontSize = 14;
            if (regularFont != null) itemLabelText.font = regularFont;
            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = new Vector2(0.05f, 0.1f);
            itemLabelRect.anchorMax = new Vector2(0.95f, 0.9f);
            itemLabelRect.offsetMin = Vector2.zero;
            itemLabelRect.offsetMax = Vector2.zero;

            item.AddComponent<Toggle>();

            TMP_Dropdown dropdown = obj.AddComponent<TMP_Dropdown>();
            dropdown.template = templateRect;
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(options));

            template.SetActive(false);

            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, float yPos, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, yPos - 0.11f);
            rect.anchorMax = new Vector2(0.95f, yPos);

            Image bg = obj.AddComponent<Image>();
            bg.color = color;

            UnityEngine.UI.Shadow shadow = obj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.4f);
            shadow.effectDistance = new Vector2(0, -3);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            if (regularFont != null) tmp.font = regularFont;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = bg;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            return obj;
        }

        private static GameObject CreateScrollView(Transform parent, string name)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent, false);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.1f, 0.1f, 0.14f, 1f);

            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.spacing = 5;
            layout.padding = new RectOffset(5, 5, 5, 5);

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return scrollView;
        }

        private static GameObject CreateErrorPanel(Transform parent)
        {
            GameObject panel = new GameObject("ErrorPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.02f);
            rect.anchorMax = new Vector2(0.95f, 0.08f);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

            GameObject errorText = CreateText(panel.transform, "ErrorText", "", 12, TextAlignmentOptions.Left);
            RectTransform errorRect = errorText.GetComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0.02f, 0.1f);
            errorRect.anchorMax = new Vector2(0.98f, 0.9f);
            errorRect.offsetMin = Vector2.zero;
            errorRect.offsetMax = Vector2.zero;

            panel.SetActive(false);

            return panel;
        }

        private static GameObject CreateJobItemPrefab()
        {
            GameObject prefab = new GameObject("JobItemPrefab");

            RectTransform rect = prefab.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 36);

            Image bg = prefab.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.18f, 0.22f, 1f);

            UnityEngine.UI.Outline outline = prefab.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 0.3f);
            outline.effectDistance = new Vector2(0, -1);

            HorizontalLayoutGroup layout = prefab.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 10;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            GameObject text = new GameObject("Text");
            text.transform.SetParent(prefab.transform, false);
            TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            if (regularFont != null) tmp.font = regularFont;
            LayoutElement textLayout = text.AddComponent<LayoutElement>();
            textLayout.flexibleWidth = 1;

            GameObject deleteButton = new GameObject("DeleteButton");
            deleteButton.transform.SetParent(prefab.transform, false);
            RectTransform btnRect = deleteButton.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(70, 26);
            Image btnBg = deleteButton.AddComponent<Image>();
            btnBg.color = new Color(0.85f, 0.3f, 0.3f, 1f);

            UnityEngine.UI.Shadow btnShadow = deleteButton.AddComponent<UnityEngine.UI.Shadow>();
            btnShadow.effectColor = new Color(0, 0, 0, 0.3f);
            btnShadow.effectDistance = new Vector2(0, -2);

            Button btn = deleteButton.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            ColorBlock btnColors = btn.colors;
            btnColors.normalColor = Color.white;
            btnColors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            btnColors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btnColors.colorMultiplier = 1f;
            btn.colors = btnColors;

            GameObject btnText = new GameObject("Text");
            btnText.transform.SetParent(deleteButton.transform, false);
            TextMeshProUGUI btnTmp = btnText.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "삭제";
            btnTmp.fontSize = 12;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;
            if (regularFont != null) btnTmp.font = regularFont;
            RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;

            LayoutElement btnLayout = deleteButton.AddComponent<LayoutElement>();
            btnLayout.minWidth = 70;
            btnLayout.preferredWidth = 70;

            return prefab;
        }

        private static GameObject CreateGridInfoPanel(Transform parent)
        {
            GameObject panel = new GameObject("CellInfoPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.75f);
            rect.anchorMax = new Vector2(1, 0.95f);
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.20f, 0.95f);

            UnityEngine.UI.Shadow shadow = panel.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);

            GameObject title = CreateText(panel.transform, "Title", "셀 정보", 18, TextAlignmentOptions.Left, true);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(15, 0);
            titleRect.offsetMax = new Vector2(-15, -5);

            float yPos = 0.65f;
            GameObject cellCodeText = CreateText(panel.transform, "CellCodeText", "셀: -", 14, TextAlignmentOptions.Left);
            RectTransform codeRect = cellCodeText.GetComponent<RectTransform>();
            codeRect.anchorMin = new Vector2(0, yPos);
            codeRect.anchorMax = new Vector2(1, yPos + 0.12f);
            codeRect.offsetMin = new Vector2(15, 0);
            codeRect.offsetMax = new Vector2(-15, 0);

            yPos -= 0.15f;
            GameObject accessText = CreateText(panel.transform, "AccessibilityText", "접근 가능", 13, TextAlignmentOptions.Left);
            RectTransform accessRect = accessText.GetComponent<RectTransform>();
            accessRect.anchorMin = new Vector2(0, yPos);
            accessRect.anchorMax = new Vector2(0.5f, yPos + 0.1f);
            accessRect.offsetMin = new Vector2(15, 0);
            accessRect.offsetMax = new Vector2(-5, 0);

            GameObject dimensionsText = CreateText(panel.transform, "DimensionsText", "치수: -", 13, TextAlignmentOptions.Left);
            RectTransform dimRect = dimensionsText.GetComponent<RectTransform>();
            dimRect.anchorMin = new Vector2(0.5f, yPos);
            dimRect.anchorMax = new Vector2(1, yPos + 0.1f);
            dimRect.offsetMin = new Vector2(5, 0);
            dimRect.offsetMax = new Vector2(-15, 0);

            yPos -= 0.15f;
            GameObject capacityText = CreateText(panel.transform, "CapacityText", "용량: -", 13, TextAlignmentOptions.Left);
            RectTransform capRect = capacityText.GetComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0, yPos);
            capRect.anchorMax = new Vector2(1, yPos + 0.1f);
            capRect.offsetMin = new Vector2(15, 0);
            capRect.offsetMax = new Vector2(-15, 0);

            yPos -= 0.15f;
            GameObject bookInfoText = CreateText(panel.transform, "BookInfoText", "보관 도서: 없음", 13, TextAlignmentOptions.Left);
            RectTransform bookRect = bookInfoText.GetComponent<RectTransform>();
            bookRect.anchorMin = new Vector2(0, yPos);
            bookRect.anchorMax = new Vector2(1, yPos + 0.1f);
            bookRect.offsetMin = new Vector2(15, 0);
            bookRect.offsetMax = new Vector2(-15, 0);

            CellInfoPanel infoPanel = panel.AddComponent<CellInfoPanel>();
            typeof(CellInfoPanel).GetField("cellCodeText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, cellCodeText.GetComponent<TextMeshProUGUI>());
            typeof(CellInfoPanel).GetField("accessibilityText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, accessText.GetComponent<TextMeshProUGUI>());
            typeof(CellInfoPanel).GetField("dimensionsText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, dimensionsText.GetComponent<TextMeshProUGUI>());
            typeof(CellInfoPanel).GetField("capacityText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, capacityText.GetComponent<TextMeshProUGUI>());
            typeof(CellInfoPanel).GetField("bookInfoText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, bookInfoText.GetComponent<TextMeshProUGUI>());
            typeof(CellInfoPanel).GetField("panelObject",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(infoPanel, panel);

            EditorUtility.SetDirty(infoPanel);

            panel.SetActive(false);

            return panel;
        }

        private static GameObject CreateGridControlPanel(Transform parent)
        {
            GameObject panel = new GameObject("GridControlPanel");
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.08f);
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

            GameObject pauseButton = CreateButton(panel.transform, "PauseResumeButton", "일시정지", 0.5f, new Color(0.9f, 0.6f, 0.2f));
            RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(0.02f, 0.15f);
            pauseRect.anchorMax = new Vector2(0.22f, 0.85f);
            pauseRect.offsetMin = Vector2.zero;
            pauseRect.offsetMax = Vector2.zero;

            GameObject stopButton = CreateButton(panel.transform, "StopButton", "정지", 0.5f, new Color(0.85f, 0.25f, 0.25f));
            RectTransform stopRect = stopButton.GetComponent<RectTransform>();
            stopRect.anchorMin = new Vector2(0.24f, 0.15f);
            stopRect.anchorMax = new Vector2(0.40f, 0.85f);
            stopRect.offsetMin = Vector2.zero;
            stopRect.offsetMax = Vector2.zero;

            GameObject timeText = CreateText(panel.transform, "ElapsedTimeText", "경과 시간: 0.0s", 14, TextAlignmentOptions.Left, true);
            RectTransform timeRect = timeText.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.45f, 0);
            timeRect.anchorMax = new Vector2(1, 1);
            timeRect.offsetMin = new Vector2(10, 0);
            timeRect.offsetMax = new Vector2(-10, 0);

            DashboardUI dashboard = panel.AddComponent<DashboardUI>();
            typeof(DashboardUI).GetField("pauseResumeButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dashboard, pauseButton.GetComponent<Button>());

            var pauseButtonText = pauseButton.transform.Find("Text");
            if (pauseButtonText != null)
            {
                typeof(DashboardUI).GetField("pauseResumeButtonText",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(dashboard, pauseButtonText.GetComponent<TextMeshProUGUI>());
            }

            typeof(DashboardUI).GetField("stopButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dashboard, stopButton.GetComponent<Button>());

            EditorUtility.SetDirty(dashboard);

            return panel;
        }
    }
}
