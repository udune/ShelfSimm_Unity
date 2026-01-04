using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class JobInputData
    {
        [Header("작업 정보")]
        public string cellCodesText = "";
        public JobAction actionType = JobAction.PUT;
        public string materialId = "";
        public int quantity = 1;

        [Header("파싱 결과")]
        public List<string> parsedCodes = new();
        public List<string> invalidCodes = new();
    }

    [Serializable]
    public struct InputValidationResult
    {
        public bool IsValid;
        public bool HasCellCodes;
        public bool HasValidMaterial;
        public bool HasValidQuantity;
        public int CorrectedQuantity;
        public List<string> ErrorMessages;

        public static InputValidationResult Invalid(List<string> errors = null)
        {
            return new InputValidationResult
            {
                IsValid = false,
                ErrorMessages = errors ?? new List<string>()
            };
        }

        public static InputValidationResult Valid(int correctedQuantity)
        {
            return new InputValidationResult
            {
                IsValid = true,
                HasCellCodes = true,
                HasValidMaterial = true,
                HasValidQuantity = true,
                CorrectedQuantity = correctedQuantity,
                ErrorMessages = new List<string>()
            };
        }
    }

    public static class InputValidator
    {
        private const int DEFAULT_QUANTITY = 1;
        private const int MIN_QUANTITY = 1;
        private const int MAX_QUANTITY = 999;

        public static int CorrectQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                return DEFAULT_QUANTITY;
            }

            if (quantity > MAX_QUANTITY)
            {
                return MAX_QUANTITY;
            }

            return quantity;
        }

        public static InputValidationResult ValidateJobInput(JobInputData jobInput)
        {
            if (jobInput == null)
            {
                return InputValidationResult.Invalid(new List<string> { "입력 데이터가 null입니다." });
            }

            var errors = new List<string>();
            var hasCellCodes = false;
            var hasValidMaterial = false;
            var hasValidQuantity = true;

            if (string.IsNullOrWhiteSpace(jobInput.cellCodesText))
            {
                errors.Add("칸 코드를 입력해주세요. (예: D20, A15)");
            }
            else if (jobInput.parsedCodes == null || jobInput.parsedCodes.Count == 0)
            {
                errors.Add("유효한 칸 코드가 없습니다.");
            }
            else
            {
                hasCellCodes = true;

                if (jobInput.invalidCodes != null && jobInput.invalidCodes.Count > 0)
                {
                    errors.Add($"알 수 없는 코드: {string.Join(", ", jobInput.invalidCodes)}");
                }
            }

            if (string.IsNullOrWhiteSpace(jobInput.materialId))
            {
                errors.Add("자재를 선택해주세요.");
            }
            else
            {
                hasValidMaterial = true;
            }

            var correctedQuantity = CorrectQuantity(jobInput.quantity);
            var isValid = errors.Count == 0 && hasCellCodes && hasValidMaterial;

            return new InputValidationResult
            {
                IsValid = isValid,
                HasCellCodes = hasCellCodes,
                HasValidMaterial = hasValidMaterial,
                HasValidQuantity = hasValidQuantity,
                CorrectedQuantity = correctedQuantity,
                ErrorMessages = errors
            };
        }

        public static bool IsEnableExecuteButton(JobInputData jobInput)
        {
            var validationResult = ValidateJobInput(jobInput);
            return validationResult.IsValid;
        }
    }
}