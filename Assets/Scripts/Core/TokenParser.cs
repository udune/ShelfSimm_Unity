using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Core
{
    // 토큰 파싱 결과 구조체
    [Serializable]
    public struct TokenParseResult
    {
        public string[] Tokens; // 파싱된 토큰 배열
        public int ValidTokenCount; // 유효한 토큰의 개수
        public string[] ErrorMessages; // 각 토큰에 대한 오류 메시지 배열
    }
    
    public static class TokenParser
    {
        private static readonly char[] Separators = { ',', ' ', '\t', '\n', '\r' }; // 구분자 배열
        
        private static readonly Regex MultipleSpacesRegex = new Regex(@"\s*,\s*|\s+", RegexOptions.Compiled); // 다중 공백 및 쉼표 정규식

        // 입력 문자열을 토큰으로 파싱하는 메서드
        public static string[] ParseTokens(string input)
        {
            if (string.IsNullOrEmpty(input)) // 입력이 null 또는 빈 문자열인 경우 빈 배열 반환
            {
                return Array.Empty<string>();
            }

            string trimmed = input.Trim(); // 앞뒤 공백 제거
            if (string.IsNullOrEmpty(trimmed)) // 공백만 있는 경우 빈 배열
            {
                return Array.Empty<string>();
            }
            
            string normalized = MultipleSpacesRegex.Replace(trimmed, ","); // 다중 공백 및 쉼표를 단일 쉼표로 변환
            
            string[] tokens = normalized.Split(',', StringSplitOptions.RemoveEmptyEntries); // 쉼표로 분리하여 토큰 배열 생성
            
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim(); // 각 토큰의 앞뒤 공백 제거
            }
            
            List<string> validTokens = new List<string>();
            foreach (string token in tokens)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    validTokens.Add(token); // 유효한 토큰만 리스트에 추가
                }
            }
            
            return validTokens.ToArray(); // 유효한 토큰 배열 반환
        }
        
        // 입력 문자열을 파싱하고 정규화하는 메서드
        public static TokenParseResult ParseAndNormalize(string input)
        {
            string[] tokens = ParseTokens(input); // 토큰 파싱

            if (tokens.Length == 0) // 파싱된 토큰이 없는 경우
            {
                return new TokenParseResult
                {
                    Tokens = Array.Empty<string>(), // 빈 토큰 배열
                    ValidTokenCount = 0, // 유효한 토큰 개수 0
                    ErrorMessages = Array.Empty<string>() // 빈 오류 메시지 배열
                };
            }
            
            string[] normalizedTokens = new string[tokens.Length]; // 정규화된 토큰 배열
            string[] errorMessages = new string[tokens.Length]; // 각 토큰에 대한 오류 메시지 배열
            int validCount = 0; // 유효한 토큰 개수

            for (int i = 0; i < tokens.Length; i++)
            {
                try
                {
                    normalizedTokens[i] = CodeNormalizer.NormalizeCode(tokens[i]); // 코드 정규화 시도
                    errorMessages[i] = null; // 오류 없음
                    validCount++; // 유효한 토큰 개수 증가
                }
                catch (ArgumentException ex)
                {
                    normalizedTokens[i] = tokens[i]; // 원래 토큰 유지
                    errorMessages[i] = ex.Message; // 오류 메시지 저장
                }
            }

            return new TokenParseResult()
            {
                Tokens = normalizedTokens, // 정규화된 토큰 배열
                ValidTokenCount = validCount, // 유효한 토큰 개수
                ErrorMessages = errorMessages // 각 토큰에 대한 오류 메시지 배열
            };
        }

        // 입력 문자열의 토큰 개수를 세는 메서드
        public static int CountTokens(string input)
        {
            return ParseTokens(input).Length; // 파싱된 토큰 배열의 길이 반환
        }

        // 입력 문자열에 유효한 토큰이 하나라도 있는지 확인하는 메서드
        public static bool HasValidTokens(string input)
        {
            var result = ParseAndNormalize(input); // 토큰 파싱 및 정규화
            return result.ValidTokenCount > 0; // 유효한 토큰이 하나라도 있는지 확인
        }
    }
}
