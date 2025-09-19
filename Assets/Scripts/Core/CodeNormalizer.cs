using System;
using System.Text.RegularExpressions;

namespace Core
{
    // 코드 정규화 결과 구조체
    [Serializable]
    public struct CodeNormalizationResult
    {
        public string OriginalCode; // 원본 코드
        public string NormalizedCode; // 정규화된 코드
        public bool IsValid; // 유효성 여부
        public string ErrorMessage; // 오류 메시지
    }
    
    // 코드 정규화 유틸리티 클래스
    public static class CodeNormalizer
    {
        // 코드 패턴 정규식 (대문자 1자리 + 숫자 1~2자리)
        private static readonly Regex CodePattern = new Regex(@"^([A-Z])(\d+)$", RegexOptions.Compiled);
        
        // 코드 정규화 메서드
        public static string NormalizeCode(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode))
            {
                throw new ArgumentException("코드가 비어있습니다", nameof(rawCode));
            }
            
            string cleaned = rawCode // 1. 앞뒤 공백 제거, 2. 대문자 변환, 3. 공백/하이픈/언더스코어 제거
                .Trim() 
                .ToUpper()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");
            
            var match = CodePattern.Match(cleaned); // 3. 패턴 매칭
            if (!match.Success)
            {
                throw new ArgumentException($"잘못된 코드 형식입니다: {rawCode}", nameof(rawCode));
            }
            
            string alphabetPart = match.Groups[1].Value; // 4. 알파벳 부분 추출
            string numberPart = match.Groups[2].Value; // 4. 숫자 부분 추출
            
            if (numberPart.Length > 2) // 5. 숫자부 길이 검증
            {
                throw new ArgumentException($"숫자 부분이 2자리를 초과합니다: {rawCode}", nameof(rawCode));
            }
            
            string paddedNumber = numberPart.PadLeft(2, '0'); // 6. 숫자부 2자리로 패딩
            
            return alphabetPart + paddedNumber; // 7. 정규화된 코드 반환
        }

        // 코드 정규화 시도 메서드
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

        // 다수의 코드 정규화 메서드
        public static CodeNormalizationResult[] NormalizeCodes(string[] rawCodes)
        {
            if (rawCodes == null)
            {
                return Array.Empty<CodeNormalizationResult>(); // null 입력 시 빈 배열 반환
            }

            var results = new CodeNormalizationResult[rawCodes.Length]; // 결과 배열 초기화
            
            // 각 코드에 대해 정규화 시도
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
