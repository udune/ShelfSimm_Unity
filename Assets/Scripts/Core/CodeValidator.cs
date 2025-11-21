using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    public class CodeValidator : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private CodeRegistry codeRegistry;

        public void Start()
        {
            if (codeRegistry == null)
            {
                codeRegistry = FindObjectOfType<CodeRegistry>();

                if (codeRegistry == null)
                {
                    Debug.LogError("[CodeValidator] CodeRegistry를 찾을 수 없습니다!");
                }
            }
        }

        public CodeValidationResult[] ValidateCodes(string[] inputCodes)
        {
            if (inputCodes == null || inputCodes.Length == 0)
            {
                return Array.Empty<CodeValidationResult>();
            }

            List<CodeValidationResult> results = new List<CodeValidationResult>();

            foreach (string code in inputCodes)
            {
                CodeValidationResult result = ValidateSingleCode(code);
                results.Add(result);
            }

            return results.ToArray();
        }

        public CodeValidationResult ValidateSingleCode(string inputCode)
        {
            string cleanedCode = CleanInputCode(inputCode);

            if (!CodeNormalizer.TryNormalizeCode(cleanedCode, out string normalizedCode))
            {
                return CodeValidationResult.Failure(
                    inputCode,
                    ErrorCode.INVALID_CODE,
                    $"잘못된 코드 형식입니다: {inputCode}"
                    );
            }

            if (codeRegistry == null || !codeRegistry.IsValidCode(normalizedCode))
            {
                return CodeValidationResult.Failure(
                    inputCode,
                    ErrorCode.INVALID_CODE,
                    $"등록되지 않은 코드입니다: {normalizedCode}"
                    );
            }

            return CodeValidationResult.Success(inputCode, normalizedCode);
        }

        private string CleanInputCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.Trim()
                .Replace(",", "")
                .Replace(";", "")
                .Replace(".", "")
                .Trim();
        }
    }
}