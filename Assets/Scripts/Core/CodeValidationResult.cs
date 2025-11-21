using System;
using Data;

namespace Core
{
    [Serializable]
    public struct CodeValidationResult
    {
        public string OriginalCode;
        public string NormalizedCode;
        public bool IsValid;
        public string ErrorMessage;
        public ErrorCode ErrorCode;

        public static CodeValidationResult Success(string original, string normalized)
        {
            return new CodeValidationResult
            {
                OriginalCode = original,
                NormalizedCode = normalized,
                IsValid = true,
                ErrorMessage = null,
                ErrorCode = ErrorCode.INVALID_VALUE
            };
        }

        public static CodeValidationResult Failure(string original, ErrorCode errorCode, string message)
        {
            return new CodeValidationResult
            {
                OriginalCode = original,
                NormalizedCode = null,
                IsValid = false,
                ErrorMessage = message,
                ErrorCode = errorCode
            };
        }
    }
}
