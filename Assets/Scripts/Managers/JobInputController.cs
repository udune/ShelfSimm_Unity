using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JobInputController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField cellCodesInput;
        [SerializeField] private TMP_Dropdown actionTypeDropdown;
        [SerializeField] private TMP_Dropdown bookDropdown;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button executeButton;

        [Header("Setting")]
        [SerializeField] private Color validColor = new Color(0.2f, 0.2f, 0.25f, 1);
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
            }

            currentJobInput.quantity = 1;
            isInitialized = true;
            UpdateUI();
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
            
            if (books != null && books.Length > 0)
            {
                options.AddRange(books.Select(book => book.DisplayText));
            }

            bookDropdown.AddOptions(options);
            bookDropdown.value = 0;
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
                    if (IsValidBookshelfCell(normalized))
                    {
                        currentJobInput.parsedCodes.Add(normalized);
                    }
                    else
                    {
                        currentJobInput.invalidCodes.Add(trimmed);
                    }
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
                currentJobInput.bookId = $"BOOK_{value}";
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
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (!isInitialized)
            {
                return;
            }

            var validation = InputValidator.ValidateJobInput(currentJobInput);

            if (executeButton != null)
            {
                bool isEnable = InputValidator.IsEnableExecuteButton(currentJobInput);
                executeButton.interactable = isEnable;

                var colors = executeButton.colors;
                colors.normalColor = isEnable ? Color.green : Color.gray;
                executeButton.colors = colors;
            }

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