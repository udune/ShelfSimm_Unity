using System;
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
        [SerializeField] private Toggle inboundToggle;
        [SerializeField] private Toggle outboundToggle;
        [SerializeField] private TMP_InputField materialIdInput;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button executeButton;

        [Header("Setting")] [SerializeField] private Color validColor = new(0.2f, 0.2f, 0.25f, 1);
        [SerializeField] private Color invalidColor = Color.red;
        [SerializeField] private Color validMaterialIdColor = new(0.2f, 0.8f, 0.2f, 0.3f);
        [SerializeField] private Color invalidMaterialIdColor = new(0.8f, 0.2f, 0.2f, 0.3f);

        [Header("References")] [SerializeField]
        private MaterialRegistry materialRegistry;

        private JobInputData currentJobInput = new();
        private bool isInitialized;

        public Action<JobInputData> OnExecuteRequested;

        private void Start()
        {
            Init(); // 초기화
        }

        private void Init()
        {
            inboundToggle.isOn = true;
            outboundToggle.isOn = false;
            currentJobInput.actionType = JobAction.PUT;

            inboundToggle.onValueChanged.AddListener(OnPutToggleChanged);
            outboundToggle.onValueChanged.AddListener(OnPickToggleChanged);

            quantityInput.text = "1";
            quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            quantityInput.onValueChanged.AddListener(OnQuantityChanged);
            quantityInput.onEndEdit.AddListener(OnQuantityEndEdit);

            cellCodesInput.onValueChanged.AddListener(OnCellCodesChanged);

            materialIdInput.contentType = TMP_InputField.ContentType.Standard;
            materialIdInput.onValueChanged.AddListener(OnMaterialIdChanged);
            materialIdInput.onEndEdit.AddListener(OnMaterialIdEndEdit);

            currentJobInput.quantity = 1;
            isInitialized = true;
            UpdateUI();
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
                    if (IsValidMaterialShelfCell(normalized))
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

        private bool IsValidMaterialShelfCell(string cellCode)
        {
            return ConfigManager.Instance.CellsLayout.GetCellByCode(cellCode) != null;
        }

        private void OnPutToggleChanged(bool isOn)
        {
            if (!isInitialized) return;

            if (isOn)
            {
                outboundToggle.isOn = false;
                currentJobInput.actionType = JobAction.PUT;
                UpdateUI();
            }
        }

        private void OnPickToggleChanged(bool isOn)
        {
            if (!isInitialized) return;

            if (isOn)
            {
                inboundToggle.isOn = false;
                currentJobInput.actionType = JobAction.PICK;
                UpdateUI();
            }
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

        private void OnMaterialIdChanged(string input)
        {
            if (!isInitialized) return;

            currentJobInput.materialId = input?.Trim() ?? "";
            UpdateUI();
        }

        private void OnMaterialIdEndEdit(string input)
        {
            if (!isInitialized) return;

            var trimmed = input?.Trim() ?? "";
            currentJobInput.materialId = trimmed;
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

            var cellImage = cellCodesInput.GetComponent<Image>();
            if (cellImage != null)
            {
                if (currentJobInput.invalidCodes != null && currentJobInput.invalidCodes.Count > 0)
                    cellImage.color = invalidColor;
                else
                    cellImage.color = validColor;
            }

            var materialImage = materialIdInput.GetComponent<Image>();
            if (materialImage != null)
            {
                if (string.IsNullOrWhiteSpace(currentJobInput.materialId))
                {
                    materialImage.color = validColor;
                }
                else
                {
                    bool isValidMaterial = materialRegistry != null && materialRegistry.GetMaterialByLotId(currentJobInput.materialId) != null;
                    materialImage.color = isValidMaterial ? validMaterialIdColor : invalidMaterialIdColor;
                }
            }
        }

        public JobInputData GetCurrentJobInput()
        {
            return currentJobInput;
        }

        public void ResetInput()
        {
            currentJobInput = new JobInputData { quantity = 1, actionType = JobAction.PUT };
            cellCodesInput.text = "";
            inboundToggle.isOn = true;
            outboundToggle.isOn = false;
            materialIdInput.text = "";
            quantityInput.text = "1";
            UpdateUI();
        }
    }
}