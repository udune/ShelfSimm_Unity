using System;
using System.Collections.Generic;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CodeInputHighlighter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private TextMeshProUGUI errorMessageText;
        [SerializeField] private Transform codeContainer;
        [SerializeField] private GameObject itemPrefab;

        [Header("Colors")]
        [SerializeField] private Color validColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;
        [SerializeField] private Color normalColor = Color.white;

        [Header("References")]
        [SerializeField] private CodeValidator codeValidator;
        [SerializeField] private BookDropdownController bookDropdownController;

        private List<GameObject> currentCodeItems = new List<GameObject>();

        private void Start()
        {
            if (codeValidator == null)
            {
                codeValidator = FindObjectOfType<CodeValidator>();
            }

            if (codeInputField != null)
            {
                codeInputField.onValueChanged.AddListener(OnInputChanged);
            }

            if (bookDropdownController == null)
            {
                bookDropdownController = FindObjectOfType<BookDropdownController>();
            }
        }

        private void OnInputChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ClearCodeList();
                ClearErrorMessage();
                return;
            }

            string[] codes = ParseCodes(value);
            ValidateAndHighlight(codes);
        }

        private string[] ParseCodes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            string[] separators = new string[] { ",", " ", "\t" };
            string[] rawCodes = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            return rawCodes;
        }

        public void ValidateAndHighlight(string[] codes)
        {
            if (codeValidator == null)
            {
                Debug.LogError("[CodeInputHighlighter] CodeValidator가 없습니다!");
                return;
            }

            ClearCodeList();
            ClearErrorMessage();

            CodeValidationResult[] results = codeValidator.ValidateCodes(codes);

            UpdateCodeListUI(results);
            UpdateErrorMessage(results);
        }

        private void ClearCodeList()
        {
            foreach (GameObject item in currentCodeItems)
            {
                if (item != null)
                {
                    DestroyImmediate(item);
                }
            }

            currentCodeItems.Clear();
        }

        private void ClearErrorMessage()
        {
            if (errorMessageText != null)
            {
                errorMessageText.text = "";
            }
        }

        private void UpdateCodeListUI(CodeValidationResult[] results)
        {
            if (codeContainer == null || itemPrefab == null)
            {
                return;
            }

            foreach (CodeValidationResult result in results)
            {
                GameObject item = Instantiate(itemPrefab, codeContainer);
                currentCodeItems.Add(item);

                TextMeshProUGUI codeText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (codeText != null)
                {
                    codeText.text = result.OriginalCode;
                    codeText.color = result.IsValid ? validColor : invalidColor;
                }

                Image background = item.GetComponent<Image>();
                if (background != null && !result.IsValid)
                {
                    Color backgroundColor = invalidColor;
                    backgroundColor.a = 0.3f;
                    background.color = backgroundColor;
                }
            }
        }

        private void UpdateErrorMessage(CodeValidationResult[] results)
        {
            if (errorMessageText == null)
            {
                return;
            }

            List<string> errorMessages = new List<string>();

            foreach (CodeValidationResult result in results)
            {
                if (!result.IsValid)
                {
                    errorMessages.Add(result.ErrorMessage);
                }
            }

            if (errorMessages.Count > 0)
            {
                errorMessageText.text = string.Join("\n", errorMessages);
                errorMessageText.color = invalidColor;
            }
            else
            {
                errorMessageText.text = "";
            }
        }

        public void TriggerValidation()
        {
            if (codeInputField != null)
            {
                OnInputChanged(codeInputField.text);
            }
        }

        public BookData GetSelectedBook()
        {
            if (bookDropdownController == null)
            {
                return null;
            }

            return bookDropdownController.GetSelectedBook();
        }

        public bool HasSelectedBook()
        {
            if (bookDropdownController == null)
            {
                return false;
            }

            return bookDropdownController.HasSelectedBook();
        }
    }
}