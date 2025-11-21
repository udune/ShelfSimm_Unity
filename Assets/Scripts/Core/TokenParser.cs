using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Core
{
    [Serializable]
    public struct TokenParseResult
    {
        public string[] Tokens;
        public int ValidTokenCount;
        public string[] ErrorMessages;
    }

    public static class TokenParser
    {
        private static readonly char[] Separators = { ',', ' ', '\t', '\n', '\r' };
        private static readonly Regex MultipleSpacesRegex = new Regex(@"\s*,\s*|\s+", RegexOptions.Compiled);

        public static string[] ParseTokens(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Array.Empty<string>();
            }

            string trimmed = input.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return Array.Empty<string>();
            }

            string normalized = MultipleSpacesRegex.Replace(trimmed, ",");
            string[] tokens = normalized.Split(',', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim();
            }

            List<string> validTokens = new List<string>();
            foreach (string token in tokens)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    validTokens.Add(token);
                }
            }

            return validTokens.ToArray();
        }

        public static TokenParseResult ParseAndNormalize(string input)
        {
            string[] tokens = ParseTokens(input);

            if (tokens.Length == 0)
            {
                return new TokenParseResult
                {
                    Tokens = Array.Empty<string>(),
                    ValidTokenCount = 0,
                    ErrorMessages = Array.Empty<string>()
                };
            }

            string[] normalizedTokens = new string[tokens.Length];
            string[] errorMessages = new string[tokens.Length];
            int validCount = 0;

            for (int i = 0; i < tokens.Length; i++)
            {
                try
                {
                    normalizedTokens[i] = CodeNormalizer.NormalizeCode(tokens[i]);
                    errorMessages[i] = null;
                    validCount++;
                }
                catch (ArgumentException ex)
                {
                    normalizedTokens[i] = tokens[i];
                    errorMessages[i] = ex.Message;
                }
            }

            return new TokenParseResult()
            {
                Tokens = normalizedTokens,
                ValidTokenCount = validCount,
                ErrorMessages = errorMessages
            };
        }

        public static int CountTokens(string input)
        {
            return ParseTokens(input).Length;
        }

        public static bool HasValidTokens(string input)
        {
            var result = ParseAndNormalize(input);
            return result.ValidTokenCount > 0;
        }
    }
}
