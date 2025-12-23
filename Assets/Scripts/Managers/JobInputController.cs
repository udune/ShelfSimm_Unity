using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class JobInputController : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private TMP_InputField cellCodesInput;
        [SerializeField] private TMP_Dropdown actionTypeDropdown;
        [SerializeField] private TMP_Dropdown bookDropdown;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button executeButton;

        [Header("Setting")] [SerializeField] private Color validColor = new(0.2f, 0.2f, 0.25f, 1);
        [SerializeField] private Color invalidColor = Color.red;

        [Header("References")] [SerializeField]
        private BookRegistry bookRegistry;

        private JobInputData currentJobInput = new();
        private bool isInitialized;

        public Action<JobInputData> OnExecuteRequested;

        private void Start()
        {
            Init(); // 초기화
        }

        private void Init()
        {
            actionTypeDropdown.ClearOptions();
            actionTypeDropdown.AddOptions(new List<string> { "PUT", "PICK" });
            actionTypeDropdown.value = 0;
            actionTypeDropdown.onValueChanged.AddListener(OnActionTypeChanged);

            quantityInput.text = "1";
            quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            quantityInput.onValueChanged.AddListener(OnQuantityChanged);
            quantityInput.onEndEdit.AddListener(OnQuantityEndEdit);

            cellCodesInput.onValueChanged.AddListener(OnCellCodesChanged);
            cellCodesInput.placeholder.GetComponent<TextMeshProUGUI>().text = "예: D20, A15, B03";

            bookDropdown.onValueChanged.AddListener(OnBookChanged);

            currentJobInput.quantity = 1;
            isInitialized = true;
            UpdateUI();
        }


        public void RefreshBookDropdown()
        {
            // 기존 옵션 초기화
            bookDropdown.ClearOptions();

            // 새로운 책 목록 로드
            var books = bookRegistry.GetAllAvailableBooks();
            var options = new List<string> { "도서를 선택하세요" };

            if (books != null && books.Length > 0)
            {
                options.AddRange(books.Select(book => book.DisplayText));
            }

            bookDropdown.AddOptions(options);
            bookDropdown.value = 0;
        }

        private void OnCellCodesChanged(string input)
        {
            if (!isInitialized) return;

            currentJobInput.cellCodesText = input;
            ParseCellCodes(input);
            UpdateUI();
        }

        private void ParseCellCodes(string input)
        {
            currentJobInput.parsedCodes.Clear();
            currentJobInput.invalidCodes.Clear();

            if (string.IsNullOrWhiteSpace(input)) return;

            var codes = input.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var code in codes)
            {
                var trimmed = code.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                if (CodeNormalizer.TryNormalizeCode(trimmed, out var normalized))
                {
                    if (IsValidBookshelfCell(normalized))
                        currentJobInput.parsedCodes.Add(normalized);
                    else
                        currentJobInput.invalidCodes.Add(trimmed);
                }
                else
                {
                    currentJobInput.invalidCodes.Add(trimmed);
                }
            }
        }

        private bool IsValidBookshelfCell(string cellCode)
        {
            return ConfigManager.Instance.CellsLayout.GetCellByCode(cellCode) != null;
        }

        private void OnActionTypeChanged(int value)
        {
            if (!isInitialized) return;

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
                // dropdown의 첫 번째 옵션은 "도서를 선택하세요"이므로 value - 1
                var book = bookRegistry.GetBookByIndex(value - 1);
                currentJobInput.bookId = book != null ? book.Id : "";
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
                currentJobInput.quantity = 0;
            else if (int.TryParse(input, out var value)) currentJobInput.quantity = value;

            UpdateUI();
        }

        private void OnQuantityEndEdit(string input)
        {
            if (!isInitialized) return;

            var correctedQuantity = InputValidator.CorrectQuantity(currentJobInput.quantity);

            if (correctedQuantity != currentJobInput.quantity)
            {
                currentJobInput.quantity = correctedQuantity;
                quantityInput.text = correctedQuantity.ToString();
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!isInitialized) return;

            var validation = InputValidator.ValidateJobInput(currentJobInput);

            var isEnable = InputValidator.IsEnableExecuteButton(currentJobInput);
            executeButton.interactable = isEnable;

            var colors = executeButton.colors;
            colors.normalColor = isEnable ? Color.green : Color.gray;
            executeButton.colors = colors;

            var image = cellCodesInput.GetComponent<Image>();
            if (image != null)
            {
                if (currentJobInput.invalidCodes != null && currentJobInput.invalidCodes.Count > 0)
                    image.color = invalidColor;
                else
                    image.color = validColor;
            }
        }

        public JobInputData GetCurrentJobInput()
        {
            return currentJobInput;
        }

        public void ResetInput()
        {
            currentJobInput = new JobInputData { quantity = 1 };
            cellCodesInput.text = "";
            actionTypeDropdown.value = 0;
            bookDropdown.value = 0;
            quantityInput.text = "1";
            UpdateUI();
        }
    }
}