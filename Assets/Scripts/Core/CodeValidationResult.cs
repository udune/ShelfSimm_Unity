using System;
using Data;

namespace Core
{
    // 코드 검증 결과를 나타내는 구조체
    [Serializable]
    public struct CodeValidationResult
    {
        public string OriginalCode; // 원본 코드
        public string NormalizedCode; // 정규화된 코드
        public bool IsValid; // 유효성 여부
        public string ErrorMessage; // 오류 메시지
        public ErrorCode ErrorCode; // 오류 코드

        // 성공 결과 생성 메서드
        public static CodeValidationResult Success(string original, string normalized)
        {
            return new CodeValidationResult
            {
                OriginalCode = original, // 원본 코드
                NormalizedCode = normalized, // 정규화된 코드
                IsValid = true, // 유효성 여부
                ErrorMessage = null, // 오류 메시지 없음
                ErrorCode = ErrorCode.INVALID_VALUE // 성공 시 기본값
            };
        }

        // 실패 결과 생성 메서드
        public static CodeValidationResult Failure(string original, ErrorCode errorCode, string message)
        {
            return new CodeValidationResult
            {
                OriginalCode = original, // 원본 코드
                NormalizedCode = null, // 정규화된 코드 없음
                IsValid = false, // 유효성 여부
                ErrorMessage = message, // 오류 메시지
                ErrorCode = errorCode // 오류 코드
            };
        }
    }
}
