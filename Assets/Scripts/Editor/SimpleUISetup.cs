using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UI;

namespace Editor
{
    public static class SimpleUISetup
    {
        private static TMP_FontAsset boldFont;
        private static TMP_FontAsset regularFont;

        [MenuItem("Tools/ShelfSim/Setup Simple UI", false, 20)]
        public static void SetupSimpleUI()
        {
            LoadFonts();

            Canvas canvas = FindOrCreateCanvas();

            // 좌우 분할 (60% + 40%)
            GameObject leftArea = CreateArea(canvas, "LeftArea", 0, 0, 0.6f, 1);
            GameObject rightArea = CreateArea(canvas, "RightArea", 0.6f, 0, 1, 1);

            // 오른쪽 배경
            Image rightBg = rightArea.AddComponent<Image>();
            rightBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // 오른쪽 영역을 상/중/하로 분할
            CreateControlPanels(rightArea.transform);

            Debug.Log("Simple UI 설정 완료!");
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

                canvasObj.AddComponent<GraphicRaycaster>();
            }
            return canvas;
        }

        private static void LoadFonts()
        {
            string[] guids = AssetDatabase.FindAssets("NotoSansKR-Bold t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            guids = AssetDatabase.FindAssets("Pretendard-Regular t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        private static GameObject CreateArea(Canvas canvas, string name, float xMin, float yMin, float xMax, float yMax)
        {
            GameObject area = new GameObject(name);
            area.transform.SetParent(canvas.transform, false);

            RectTransform rect = area.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.sizeDelta = Vector2.zero;

            return area;
        }

        private static void CreateControlPanels(Transform parent)
        {
            // 상단 패널 (작업 입력)
            GameObject topPanel = CreatePanel(parent, "JobInputPanel", 0, 0.67f, 1, 1);
            AddPanelBackground(topPanel, new Color(0.15f, 0.15f, 0.2f, 1f));

            VerticalLayoutGroup topLayout = topPanel.AddComponent<VerticalLayoutGroup>();
            topLayout.padding = new RectOffset(20, 20, 20, 20);
            topLayout.spacing = 10;
            topLayout.childForceExpandWidth = true;
            topLayout.childForceExpandHeight = false;
            topLayout.childControlHeight = true;

            // 타이틀
            CreateTitle(topPanel.transform, "작업 입력", 50);

            // 입력 필드들
            CreateInputRow(topPanel.transform, "셀 코드", "A01, A02-A05", 60);
            CreateDropdownRow(topPanel.transform, "작업 유형", new string[] { "PUT", "PICK" }, 60);
            CreateDropdownRow(topPanel.transform, "도서 선택", new string[] { "책 선택" }, 60);
            CreateInputRow(topPanel.transform, "수량", "1", 60);

            // 버튼
            CreateButton(topPanel.transform, "작업 추가", new Color(0.3f, 0.7f, 0.4f), 60);

            // 중간 패널 (작업 목록)
            GameObject middlePanel = CreatePanel(parent, "JobListPanel", 0, 0.33f, 1, 0.66f);
            AddPanelBackground(middlePanel, new Color(0.15f, 0.15f, 0.2f, 1f));

            VerticalLayoutGroup middleLayout = middlePanel.AddComponent<VerticalLayoutGroup>();
            middleLayout.padding = new RectOffset(20, 20, 20, 20);
            middleLayout.spacing = 10;
            middleLayout.childForceExpandWidth = true;
            middleLayout.childForceExpandHeight = false;

            CreateTitle(middlePanel.transform, "작업 목록 (0개)", 50);

            // ScrollView
            GameObject scrollView = CreateScrollView(middlePanel.transform);
            LayoutElement scrollLayout = scrollView.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;

            // 버튼들
            GameObject buttonRow = CreateHorizontalRow(middlePanel.transform, 60);
            CreateButton(buttonRow.transform, "전체 삭제", new Color(0.8f, 0.3f, 0.3f), 0);
            CreateButton(buttonRow.transform, "시뮬레이션 시작", new Color(0.3f, 0.6f, 1.0f), 0);

            // 하단 패널 (상태)
            GameObject bottomPanel = CreatePanel(parent, "StatusPanel", 0, 0, 1, 0.32f);
            AddPanelBackground(bottomPanel, new Color(0.12f, 0.12f, 0.16f, 1f));

            VerticalLayoutGroup bottomLayout = bottomPanel.AddComponent<VerticalLayoutGroup>();
            bottomLayout.padding = new RectOffset(20, 20, 20, 20);
            bottomLayout.spacing = 10;
            bottomLayout.childForceExpandWidth = true;
            bottomLayout.childForceExpandHeight = false;

            CreateTitle(bottomPanel.transform, "시뮬레이션 상태", 50);

            CreateLabel(bottomPanel.transform, "준비", 0);
        }

        private static GameObject CreatePanel(Transform parent, string name, float xMin, float yMin, float xMax, float yMax)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.sizeDelta = Vector2.zero;

            return panel;
        }

        private static void AddPanelBackground(GameObject panel, Color color)
        {
            Image bg = panel.AddComponent<Image>();
            bg.color = color;

            UnityEngine.UI.Shadow shadow = panel.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(3, -3);
        }

        private static void CreateTitle(Transform parent, string text, float height)
        {
            GameObject title = new GameObject("Title");
            title.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = title.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;
            if (boldFont != null) tmp.font = boldFont;

            LayoutElement layout = title.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
        }

        private static void CreateInputRow(Transform parent, string label, string placeholder, float height)
        {
            GameObject row = CreateHorizontalRow(parent, height);

            // 라벨
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 16;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.color = new Color(0.8f, 0.8f, 0.8f);
            if (regularFont != null) labelTmp.font = regularFont;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 150;

            // 입력 필드
            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);

            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.25f);

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            TextMeshProUGUI textTmp = textObj.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize = 16;
            if (regularFont != null) textTmp.font = regularFont;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            TextMeshProUGUI placeholderTmp = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = placeholder;
            placeholderTmp.fontSize = 16;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.color = new Color(0.5f, 0.5f, 0.5f);
            if (regularFont != null) placeholderTmp.font = regularFont;

            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);

            inputField.textViewport = inputObj.GetComponent<RectTransform>();
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;

            LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
            inputLayout.flexibleWidth = 1;
        }

        private static void CreateDropdownRow(Transform parent, string label, string[] options, float height)
        {
            GameObject row = CreateHorizontalRow(parent, height);

            // 라벨
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 16;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.color = new Color(0.8f, 0.8f, 0.8f);
            if (regularFont != null) labelTmp.font = regularFont;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 150;

            // Dropdown
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(row.transform, false);

            Image dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.2f, 0.2f, 0.25f);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(new System.Collections.Generic.List<string>(options));

            // Label
            GameObject captionObj = new GameObject("Label");
            captionObj.transform.SetParent(dropdownObj.transform, false);
            TextMeshProUGUI captionTmp = captionObj.AddComponent<TextMeshProUGUI>();
            captionTmp.fontSize = 16;
            if (regularFont != null) captionTmp.font = regularFont;

            RectTransform captionRect = captionObj.GetComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = new Vector2(10, 5);
            captionRect.offsetMax = new Vector2(-30, -5);

            dropdown.captionText = captionTmp;

            LayoutElement dropdownLayout = dropdownObj.AddComponent<LayoutElement>();
            dropdownLayout.flexibleWidth = 1;
        }

        private static void CreateButton(Transform parent, string text, Color color, float height)
        {
            GameObject button = new GameObject("Button");
            button.transform.SetParent(parent, false);

            Image bg = button.AddComponent<Image>();
            bg.color = color;

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = bg;

            // 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            if (boldFont != null) tmp.font = boldFont;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            if (height > 0)
            {
                LayoutElement layout = button.AddComponent<LayoutElement>();
                layout.preferredHeight = height;
            }
            else
            {
                LayoutElement layout = button.AddComponent<LayoutElement>();
                layout.flexibleWidth = 1;
                layout.preferredHeight = 50;
            }
        }

        private static void CreateLabel(Transform parent, string text, float height)
        {
            GameObject label = new GameObject("Label");
            label.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = new Color(0.9f, 0.9f, 0.9f);
            if (regularFont != null) tmp.font = regularFont;

            if (height > 0)
            {
                LayoutElement layout = label.AddComponent<LayoutElement>();
                layout.preferredHeight = height;
            }
            else
            {
                LayoutElement layout = label.AddComponent<LayoutElement>();
                layout.flexibleHeight = 1;
            }
        }

        private static GameObject CreateHorizontalRow(Transform parent, float height)
        {
            GameObject row = new GameObject("Row");
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            if (height > 0)
            {
                LayoutElement rowLayout = row.AddComponent<LayoutElement>();
                rowLayout.preferredHeight = height;
            }

            return row;
        }

        private static GameObject CreateScrollView(Transform parent)
        {
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(parent, false);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.1f, 0.1f, 0.14f);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            viewport.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            return scrollView;
        }
    }
}
