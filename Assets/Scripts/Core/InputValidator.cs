using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    // 작업 입력 데이터 구조체
    [Serializable]
    public class JobInputData
    {
        [Header("작업 정보")] 
        public string cellCodesText = ""; // 쉼표로 구분된 칸 코드 입력 문자열
        public JobAction actionType = JobAction.PUT; // 작업 유형 (PUT 또는 PICK)
        public string bookId = ""; // 선택된 도서 ID
        public int quantity = 1; // 작업 수량

        [Header("파싱 결과")] 
        public List<string> parsedCodes = new(); // 파싱된 칸 코드 리스트
        public List<string> invalidCodes = new(); // 유효하지 않은 칸 코드 리스트
    }

    // 작업 유형 열거형
    public enum JobAction
    {
        PUT, // 넣기
        PICK // 빼기
    }

    // 입력 검증 결과 구조체
    [Serializable]
    public struct InputValidationResult
    {
        public bool IsValid; // 전체 유효성
        public bool HasCellCodes; // 칸 코드 입력 여부
        public bool HasValidBook; // 유효한 도서 선택 여부
        public bool HasValidQuantity; // 유효한 수량 여부
        public int CorrectedQuantity; // 보정된 수량
        public List<string> ErrorMessages; // 에러 메시지 목록

        // 유효하지 않은 결과 생성 메서드
        public static InputValidationResult Invalid(List<string> errors = null)
        {
            // 에러 메시지가 null일 경우 빈 리스트로 초기화
            return new InputValidationResult
            {
                IsValid = false, // 전체 유효성 false
                ErrorMessages = errors ?? new List<string>() // 에러 메시지 설정
            };
        }

        // 유효한 결과 생성 메서드
        public static InputValidationResult Valid(int correctedQuantity)
        {
            // 모든 항목이 유효한 경우
            return new InputValidationResult
            {
                IsValid = true, // 전체 유효성 true
                HasCellCodes = true, // 칸 코드 입력 있음
                HasValidBook = true, // 유효한 도서 선택 있음
                HasValidQuantity = true, // 유효한 수량 있음
                CorrectedQuantity = correctedQuantity, // 보정된 수량 설정
                ErrorMessages = new List<string>() // 에러 메시지 없음
            };
        }
    }

    // 입력 검증 유틸리티 클래스
    public static class InputValidator
    {
        private const int DEFAULT_QUANTITY = 1; // 기본 수량
        private const int MIN_QUANTITY = 1; // 최소 수량
        private const int MAX_QUANTITY = 999; // 최대 수량

        // 수량 보정 메서드
        public static int CorrectQuantity(int quantity)
        {
            if (quantity <= 0) // 음수 또는 0인 경우
            {
                Debug.Log($"[InputValidator] 수량이 {quantity}에서 1로 보정되었습니다.");
                return DEFAULT_QUANTITY; // 기본 수량으로 보정
            }

            if (quantity > MAX_QUANTITY) // 최대 수량 초과인 경우
            {
                Debug.Log($"[InputValidator] 수량이 {quantity}에서 {MAX_QUANTITY}로 제한되었습니다.");
                return MAX_QUANTITY; // 최대 수량으로 제한
            }

            return quantity; // 유효한 수량 반환
        }

        // 작업 입력 검증 메서드
        public static InputValidationResult ValidateJobInput(JobInputData jobInput)
        {
            if (jobInput == null) // 입력 데이터가 null인 경우
            {
                return InputValidationResult.Invalid(new List<string> { "입력 데이터가 null입니다." }); // 에러 메시지 반환
            }

            var errors = new List<string>(); // 에러 메시지 리스트 초기화
            var hasCellCodes = false; // 칸 코드 입력 여부
            var hasValidBook = false; // 유효한 도서 선택 여부
            var hasValidQuantity = true; // 유효한 수량 여부
            
            if (string.IsNullOrWhiteSpace(jobInput.cellCodesText)) // 칸 코드 입력이 비어있는 경우
            {
                errors.Add("칸 코드를 입력해주세요. (예: D20, A15)"); // 에러 메시지 추가
            }
            else if (jobInput.parsedCodes == null || jobInput.parsedCodes.Count == 0) // 파싱된 칸 코드가 없는 경우
            {
                errors.Add("유효한 칸 코드가 없습니다."); // 에러 메시지 추가
            }
            else
            {
                hasCellCodes = true; // 칸 코드 입력 있음

                if (jobInput.invalidCodes != null && jobInput.invalidCodes.Count > 0) // 유효하지 않은 칸 코드가 있는 경우
                {
                    errors.Add($"알 수 없는 코드: {string.Join(", ", jobInput.invalidCodes)}"); // 에러 메시지 추가
                }
            }

            if (string.IsNullOrWhiteSpace(jobInput.bookId)) // 도서 ID가 비어있는 경우
            {
                errors.Add("도서를 선택해주세요."); // 에러 메시지 추가
            }
            else
            {
                hasValidBook = true; // 유효한 도서 선택 있음
            }

            var correctedQuantity = CorrectQuantity(jobInput.quantity); // 수량 보정
            if (correctedQuantity != jobInput.quantity) // 보정된 수량이 원래 수량과 다른 경우
            {
                Debug.Log($"[InputValidator] 수량 보정: {jobInput.quantity} -> {correctedQuantity}");
            }

            var isValid = errors.Count == 0 && hasCellCodes && hasValidBook; // 전체 유효성 판단

            return new InputValidationResult // 결과 반환
            {
                IsValid = isValid, // 전체 유효성
                HasCellCodes = hasCellCodes, // 칸 코드 입력 여부
                HasValidBook = hasValidBook, // 유효한 도서 선택 여부
                HasValidQuantity = hasValidQuantity, // 유효한 수량 여부
                CorrectedQuantity = correctedQuantity, // 보정된 수량
                ErrorMessages = errors // 에러 메시지 리스트
            };
        }

        // 실행 버튼 활성화 여부 판단 메서드
        public static bool IsEnableExecuteButton(JobInputData jobInput)
        {
            var validationResult = ValidateJobInput(jobInput); // 입력 검증
            return validationResult.IsValid; // 전체 유효성 반환
        }

        // 입력 완성도 계산 메서드
        public static float GetInputCompleteness(JobInputData jobInput)
        {
            if (jobInput == null) // 입력 데이터가 null인 경우
            {
                return 0f; // 완성도 0 반환
            }
            
            var validationResult = ValidateJobInput(jobInput); // 입력 검증
            float completeness = 0f; // 완성도 초기화
            
            if (validationResult.HasCellCodes) // 칸 코드 입력이 있는 경우
            {
                completeness += 0.5f; // 완성도 50% 증가
            }
            
            if (validationResult.HasValidBook) // 유효한 도서 선택이 있는 경우
            {
                completeness += 0.3f; // 완성도 30% 증가
            }
            
            if (validationResult.HasValidQuantity) // 유효한 수량이 있는 경우
            {
                completeness += 0.2f; // 완성도 20% 증가
            }

            return completeness; // 최종 완성도 반환
        }

        // 테스트 헬퍼 클래스
        public static class TestHelper
        {
            public static void TestQuantityCorrection() // 수량 보정 테스트 메서드
            {
                int[] testValues = { -5, 0, 1, 50, 999, 1000, 9999 }; // 테스트할 수량 값들
                
                Debug.Log("=== 수량 보정 테스트 ===");
                foreach (int value in testValues) // 각 테스트 값에 대해
                {
                    int corrected = CorrectQuantity(value); // 수량 보정
                    Debug.Log($"입력: {value}, 보정된 수량: {corrected}");
                }
            }

            public static void TestInputValidation() // 입력 검증 테스트 메서드
            {
                Debug.Log("=== 입력 검증 테스트 ===");
                
                var completeInput = new JobInputData // 완성된 입력 예시
                {
                    cellCodesText = "D20, A15", // 쉼표로 구분된 칸 코드
                    parsedCodes = new List<string> { "D20", "A15" }, // 파싱된 칸 코드
                    invalidCodes = new List<string>(), // 유효하지 않은 칸 코드 없음
                    actionType = JobAction.PUT, // 작업 유형
                    bookId = "book123", // 선택된 도서 ID
                    quantity = 2 // 작업 수량
                };
                
                var result1 = ValidateJobInput(completeInput); // 입력 검증
                Debug.Log($"완성된 입력 - 유효성: {result1.IsValid}, 완성도: {GetInputCompleteness(completeInput):P0}");
                
                var partialInput = new JobInputData // 부분 입력 예시
                {
                    cellCodesText = "D20", // 쉼표로 구분된 칸 코드
                    parsedCodes = new List<string> { "D20" }, // 파싱된 칸 코드
                    actionType = JobAction.PUT, // 작업 유형
                    quantity = 1 // 작업 수량
                };
                
                var result2 = ValidateJobInput(partialInput); // 입력 검증
                Debug.Log($"부분 입력 - 유효성: {result2.IsValid}, 완성도: {GetInputCompleteness(partialInput):P0}");
                Debug.Log($"오류 메시지: {string.Join(", ", result2.ErrorMessages)}");
            }
        }
    }
}