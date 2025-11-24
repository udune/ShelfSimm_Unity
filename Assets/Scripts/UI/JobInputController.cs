using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class JobInputController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField cellCodesInput;
        [SerializeField] private TMP_Dropdown actionTypeDropdown;
        [SerializeField] private TMP_Dropdown bookDropdown;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button executeButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider completenessSlider;

        [Header("Error")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Setting")]
        [SerializeField] private Color validColor = Color.white;
        [SerializeField] private Color invalidColor = Color.red;

        [Header("References")]
        [SerializeField] private BookRegistry bookRegistry;

        private JobInputData currentJobInput = new JobInputData();
        private bool isInitialized = false;

        public Action<JobInputData> OnValidInputChanged;
        public Action<JobInputData> OnExecuteRequested;

        private void Start()
        {
            Init(); // 초기화
        }

        private void Init()
        {
            if (actionTypeDropdown != null)
            {
                actionTypeDropdown.ClearOptions();
                actionTypeDropdown.AddOptions(new List<string> { "PUT", "PICK" });
                actionTypeDropdown.value = 0;
                actionTypeDropdown.onValueChanged.AddListener(OnActionTypeChanged);
            }

            if (quantityInput != null)
            {
                quantityInput.text = "1";
                quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
                quantityInput.onValueChanged.AddListener(OnQuantityChanged);
                quantityInput.onEndEdit.AddListener(OnQuantityEndEdit);
            }

            if (cellCodesInput != null)
            {
                cellCodesInput.onValueChanged.AddListener(OnCellCodesChanged);
                cellCodesInput.placeholder.GetComponent<TextMeshProUGUI>().text = "예: D20, A15, B03";
            }

            if (bookDropdown != null)
            {
                bookDropdown.onValueChanged.AddListener(OnBookChanged);
                LoadDummyBooks();
            }

            if (executeButton != null)
            {
                executeButton.onClick.AddListener(OnExecuteClicked);
            }

            currentJobInput.quantity = 1;
            isInitialized = true;
            UpdateUI();
        }

        private void LoadDummyBooks()
        {
            if (bookDropdown == null)
            {
                return;
            }

            if (bookRegistry == null)
            {
                bookRegistry = FindObjectOfType<BookRegistry>();
            }

            if (bookRegistry != null)
            {
                var books = bookRegistry.GetAllAvailableBooks();
                var options = new List<string> { "도서를 선택하세요" };
                options.AddRange(books.Select(book => book.DisplayText));
                bookDropdown.AddOptions(options);
            }
        }

        public void RefreshBookDropdown()
        {
            if (bookDropdown == null)
            {
                Debug.LogWarning("[JobInputController] bookDropdown is null");
                return;
            }

            if (bookRegistry == null)
            {
                bookRegistry = FindObjectOfType<BookRegistry>();
            }

            if (bookRegistry == null)
            {
                Debug.LogWarning("[JobInputController] BookRegistry not found");
                return;
            }

            // 기존 옵션 초기화
            bookDropdown.ClearOptions();

            // 새로운 책 목록 로드
            var books = bookRegistry.GetAllAvailableBooks();
            var options = new List<string> { "도서를 선택하세요" };
            options.AddRange(books.Select(book => book.DisplayText));

            bookDropdown.AddOptions(options);
            bookDropdown.value = 0;

            Debug.Log($"[JobInputController] Book dropdown refreshed with {books.Count} books");
        }

        private void OnCellCodesChanged(string input)
        {
            if (!isInitialized)
            {
                return;
            }

            currentJobInput.cellCodesText = input;
            ParseCellCodes(input);
            UpdateUI();
        }

        private void ParseCellCodes(string input)
        {
            currentJobInput.parsedCodes.Clear();
            currentJobInput.invalidCodes.Clear();

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            string[] codes = input.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

            foreach (string code in codes)
            {
                string trimmed = code.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (CodeNormalizer.TryNormalizeCode(trimmed, out string normalized))
                {
                    currentJobInput.parsedCodes.Add(normalized);
                }
                else
                {
                    currentJobInput.invalidCodes.Add(trimmed);
                }
            }
        }

        private void OnActionTypeChanged(int value)
        {
            if (!isInitialized)
            {
                return;
            }

            currentJobInput.actionType = (JobAction)value;
            UpdateUI();
        }

        private void OnBookChanged(int value)
        {
            if (!isInitialized)
            {
                return;
            }

            if (value > 0 && bookDropdown != null)
            {
                currentJobInput.bookId = $"book_{value}";
            }
            else
            {
                currentJobInput.bookId = "";
            }

            UpdateUI();
        }

        private void OnQuantityChanged(string input)
        {
            if (!isInitialized)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                currentJobInput.quantity = 0;
            }
            else if (int.TryParse(input, out int value))
            {
                currentJobInput.quantity = value;
            }

            UpdateUI();
        }

        private void OnQuantityEndEdit(string input)
        {
            if (!isInitialized || quantityInput == null)
            {
                return;
            }

            int correctedQuantity = InputValidator.CorrectQuantity(currentJobInput.quantity);

            if (correctedQuantity != currentJobInput.quantity)
            {
                currentJobInput.quantity = correctedQuantity;
                quantityInput.text = correctedQuantity.ToString();
                ShowTemporaryStatus($"수량이 {correctedQuantity}로 보정되었습니다.", 2f);
            }

            UpdateUI();
        }

        private void OnExecuteClicked()
        {
            var validation = InputValidator.ValidateJobInput(currentJobInput);

            if (validation.IsValid)
            {
                currentJobInput.quantity = validation.CorrectedQuantity;
                OnExecuteRequested?.Invoke(currentJobInput);
            }
            else
            {
                Debug.LogWarning($"[JobInputController] 입력 검증 실패: {string.Join(", ", validation.ErrorMessages)}");
            }
        }

        private void UpdateUI()
        {
            if (!isInitialized)
            {
                return;
            }

            var validation = InputValidator.ValidateJobInput(currentJobInput);
            float completeness = InputValidator.GetInputCompleteness(currentJobInput);

            if (executeButton != null)
            {
                bool isEnable = InputValidator.IsEnableExecuteButton(currentJobInput);
                executeButton.interactable = isEnable;

                var colors = executeButton.colors;
                colors.normalColor = isEnable ? Color.green : Color.gray;
                executeButton.colors = colors;
            }

            if (statusText != null)
            {
                if (validation.IsValid)
                {
                    statusText.text = "모든 입력이 유효합니다.";
                    statusText.color = validColor;
                }
                else
                {
                    statusText.text = "입력에 오류가 있습니다.";
                    statusText.color = invalidColor;
                }
            }

            if (completenessSlider != null)
            {
                completenessSlider.value = completeness;
            }

            UpdateErrorDisplay(validation.ErrorMessages);

            if (cellCodesInput != null)
            {
                var image = cellCodesInput.GetComponent<Image>();
                if (image != null)
                {
                    if (currentJobInput.invalidCodes != null && currentJobInput.invalidCodes.Count > 0)
                    {
                        image.color = invalidColor;
                    }
                    else
                    {
                        image.color = validColor;
                    }
                }
            }

            if (validation.IsValid)
            {
                OnValidInputChanged?.Invoke(currentJobInput);
            }
        }

        private void UpdateErrorDisplay(List<string> errors)
        {
            bool hasErrors = errors != null && errors.Count > 0;

            if (errorPanel != null)
            {
                errorPanel.SetActive(hasErrors);
            }

            if (errorText != null && hasErrors)
            {
                errorText.text = string.Join("\n", errors);
                errorText.color = invalidColor;
            }
        }

        private void ShowTemporaryStatus(string message, float duration)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = Color.yellow;
                Invoke(nameof(RestoreStatusText), duration);
            }
        }

        private void RestoreStatusText()
        {
            UpdateUI();
        }

        public void SetJobInput(JobInputData jobInput)
        {
            if (jobInput == null)
            {
                return;
            }

            currentJobInput = jobInput;

            if (cellCodesInput != null)
            {
                cellCodesInput.text = jobInput.cellCodesText;
            }

            if (actionTypeDropdown != null)
            {
                actionTypeDropdown.value = (int)jobInput.actionType;
            }

            if (quantityInput != null)
            {
                quantityInput.text = jobInput.quantity.ToString();
            }

            UpdateUI();
        }

        public JobInputData GetCurrentJobInput()
        {
            return currentJobInput;
        }

        public void ResetInput()
        {
            currentJobInput = new JobInputData { quantity = 1 };

            if (cellCodesInput != null)
            {
                cellCodesInput.text = "";
            }

            if (actionTypeDropdown != null)
            {
                actionTypeDropdown.value = 0;
            }

            if (bookDropdown != null)
            {
                bookDropdown.value = 0;
            }

            if (quantityInput != null)
            {
                quantityInput.text = "1";
            }

            UpdateUI();
        }
    }
}