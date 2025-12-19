using System;
using System.Text.RegularExpressions;

namespace Core
{
    [Serializable]
    public struct CodeNormalizationResult
    {
        public string OriginalCode;
        public string NormalizedCode;
        public bool IsValid;
        public string ErrorMessage;
    }

    public static class CodeNormalizer
    {
        private static readonly Regex CodePattern = new Regex(@"^([A-Z])(\d+)$", RegexOptions.Compiled);

        public static string NormalizeCode(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode))
            {
                throw new ArgumentException("코드가 비어있습니다", nameof(rawCode));
            }

            string cleaned = rawCode
                .Trim()
                .ToUpper()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");

            var match = CodePattern.Match(cleaned);
            if (!match.Success)
            {
                throw new ArgumentException($"잘못된 코드 형식입니다: {rawCode}", nameof(rawCode));
            }

            string alphabetPart = match.Groups[1].Value;
            string numberPart = match.Groups[2].Value;

            if (numberPart.Length > 2)
            {
                throw new ArgumentException($"숫자 부분이 2자리를 초과합니다: {rawCode}", nameof(rawCode));
            }

            string paddedNumber = numberPart.PadLeft(2, '0');

            return alphabetPart + paddedNumber;
        }

        public static bool TryNormalizeCode(string rawCode, out string normalizedCode)
        {
            try
            {
                normalizedCode = NormalizeCode(rawCode);
                return true;
            }
            catch
            {
                normalizedCode = null;
                return false;
            }
        }

        public static CodeNormalizationResult[] NormalizeCodes(string[] rawCodes)
        {
            if (rawCodes == null)
            {
                return Array.Empty<CodeNormalizationResult>();
            }

            var results = new CodeNormalizationResult[rawCodes.Length];

            for (int i = 0; i < rawCodes.Length; i++)
            {
                if (TryNormalizeCode(rawCodes[i], out string normalized))
                {
                    results[i] = new CodeNormalizationResult
                    {
                        OriginalCode = rawCodes[i],
                        NormalizedCode = normalized,
                        IsValid = true,
                        ErrorMessage = null
                    };
                }
                else
                {
                    results[i] = new CodeNormalizationResult
                    {
                        OriginalCode = rawCodes[i],
                        NormalizedCode = null,
                        IsValid = false,
                        ErrorMessage = $"잘못된 코드 형식: {rawCodes[i]}"
                    };
                }
            }

            return results;
        }
    }
}
