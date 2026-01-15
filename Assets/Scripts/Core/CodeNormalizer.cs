using System;

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
        public static bool TryNormalizeCode(string rawCode, out string normalizedCode)
        {
            normalizedCode = null;

            if (string.IsNullOrEmpty(rawCode))
                return false;

            // Clean the input
            var cleaned = rawCode.Trim().ToUpperInvariant();

            // Remove separators
            var sb = new System.Text.StringBuilder(cleaned.Length);
            foreach (var c in cleaned)
            {
                if (c != ' ' && c != '-' && c != '_')
                    sb.Append(c);
            }
            cleaned = sb.ToString();

            if (cleaned.Length < 2)
                return false;

            // Find where letters end and digits begin
            int letterEndIndex = 0;
            while (letterEndIndex < cleaned.Length && char.IsLetter(cleaned[letterEndIndex]))
                letterEndIndex++;

            // Must have at least one letter and one digit
            if (letterEndIndex == 0 || letterEndIndex >= cleaned.Length)
                return false;

            string alphabetPart = cleaned.Substring(0, letterEndIndex);
            string numberPart = cleaned.Substring(letterEndIndex);

            // Verify all remaining characters are digits
            foreach (var c in numberPart)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Number part should be 1-2 digits
            if (numberPart.Length == 0 || numberPart.Length > 2)
                return false;

            // Pad number to 2 digits
            string paddedNumber = numberPart.PadLeft(2, '0');
            normalizedCode = alphabetPart + paddedNumber;
            return true;
        }

        public static string NormalizeCode(string rawCode)
        {
            if (TryNormalizeCode(rawCode, out var normalized))
                return normalized;

            throw new ArgumentException($"잘못된 코드 형식입니다: {rawCode}", nameof(rawCode));
        }
    }
}