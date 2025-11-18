using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    public class CodeValidator : MonoBehaviour
    {
        [Header("참조")] 
        [SerializeField] private CodeRegistry codeRegistry; // 코드 레지스트리 참조 (인스펙터에서 수동 할당 가능)

        // 시작 시 CodeRegistry 참조 설정
        public void Start()
        {
            if (codeRegistry == null) // 수동 할당이 안 되었을 때
            {
                codeRegistry = FindObjectOfType<CodeRegistry>(); // 씬에서 CodeRegistry 컴포넌트 탐색

                if (codeRegistry == null) // 못 찾았을 때 오류 로그
                {
                    Debug.LogError("[CodeValidator] CodeRegistry를 찾을 수 없습니다!");
                }
            }
        }

        // 여러 코드를 한번에 검증 (기존 CodeNormalizer 활용)
        public CodeValidationResult[] ValidateCodes(string[] inputCodes)
        {
            if (inputCodes == null || inputCodes.Length == 0) // 입력 배열이 null이거나 비어있을 때
            {
                Debug.Log("[CodeValidator] 입력된 코드가 없습니다.");
                return Array.Empty<CodeValidationResult>(); // 빈 배열 반환
            }

            List<CodeValidationResult> results = new List<CodeValidationResult>(); // 결과 저장용 리스트
            
            Debug.Log($"[CodeValidator] {inputCodes.Length}개 코드 검증 시작");

            // 각 코드에 대해 검증 수행
            foreach (string code in inputCodes)
            {
                CodeValidationResult result = ValidateSingleCode(code); // 단일 코드 검증
                results.Add(result); // 결과 리스트에 추가

                if (result.IsValid) // 유효한 경우
                {
                    Debug.Log($"[CodeValidator] {result.OriginalCode} → {result.NormalizedCode} (유효)");
                }
                else
                {
                    Debug.LogWarning($"[CodeValidator] {result.OriginalCode} → {result.ErrorMessage}");
                }
            }
            
            return results.ToArray(); // 결과 배열 반환
        }

        // 하나의 코드 검증 (기존 CodeNormalizer + CodeRegistry 조합)
        public CodeValidationResult ValidateSingleCode(string inputCode)
        {
            string cleanedCode = CleanInputCode(inputCode); // 입력 코드 전처리
            
            if (!CodeNormalizer.TryNormalizeCode(cleanedCode, out string normalizedCode)) // 코드 정규화 시도
            {
                return CodeValidationResult.Failure(
                    inputCode, // 원본 코드
                    ErrorCode.INVALID_CODE, // 오류 코드
                    $"잘못된 코드 형식입니다: {inputCode}" // 오류 메시지
                    ); // 형식 오류
            }

            if (codeRegistry == null || !codeRegistry.IsValidCode(normalizedCode)) // 코드 레지스트리가 없거나 코드가 등록되지 않은 경우
            {
                return CodeValidationResult.Failure(
                    inputCode, // 원본 코드
                    ErrorCode.INVALID_CODE, // 오류 코드
                    $"등록되지 않은 코드입니다: {normalizedCode}" // 오류 메시지
                    ); // 등록되지 않은 코드 오류
            }
            
            // 모든 검증 통과 시 성공 결과 반환
            return CodeValidationResult.Success(inputCode, normalizedCode); // 성공 결과 반환
        }

        // 입력 코드 전처리 (쉼표, 공백 제거)
        private string CleanInputCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) // null, 빈 문자열, 공백만 있는 경우
            {
                return value; // 그대로 반환
            }

            return value.Trim() // 앞뒤 공백 제거
                .Replace(",", "") // 쉼표 제거
                .Replace(";", "") // 세미콜론 제거
                .Replace(".", "") // 마침표 제거
                .Trim(); // 다시 앞뒤 공백 제거
        }
    }
}