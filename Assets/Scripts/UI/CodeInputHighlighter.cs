using System;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 코드 입력 필드의 하이라이팅 및 오류 메시지 처리 클래스
    public class CodeInputHighlighter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_InputField codeInputField; // 입력 필드
        [SerializeField] private TextMeshProUGUI errorMessageText; // 오류 메시지 텍스트
        [SerializeField] private Transform codeContainer; // 코드 아이템 컨테이너
        [SerializeField] private GameObject itemPrefab; // 코드 아이템 프리팹
        
        [Header("Colors")]
        [SerializeField] private Color validColor = Color.green; // 유효한 코드 색상
        [SerializeField] private Color invalidColor = Color.red; // 유효하지 않은 코드 색상
        [SerializeField] private Color normalColor = Color.white; // 일반 코드 색상
        
        [Header("References")]
        [SerializeField] private CodeValidator codeValidator; // 코드 검증기 참조 (인스펙터에서 수동 할당 가능)

        private List<GameObject> currentCodeItems = new List<GameObject>(); // 현재 표시된 코드 아이템들 리스트

        private void Start()
        {
            if (codeValidator == null) // 수동 할당이 안 되었을 때
            {
                codeValidator = FindObjectOfType<CodeValidator>(); // 씬에서 CodeValidator 컴포넌트 탐색
            }

            if (codeInputField != null) // 입력 필드가 있을 때
            {
                codeInputField.onValueChanged.AddListener(OnInputChanged); // 값 변경 이벤트에 리스너 추가
            }
        }

        // 입력 필드 값 변경 시 호출되는 메서드
        private void OnInputChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) // 빈 문자열 검사
            {
                ClearCodeList(); // 코드 리스트 초기화
                ClearErrorMessage(); // 오류 메시지 초기화
                return;
            }

            string[] codes = ParseCodes(value); // 쉼표, 공백, 탭으로 분리
            ValidateAndHighlight(codes); // 코드 검증 및 하이라이트 처리
        }
        
        // 입력 문자열을 쉼표, 공백, 탭으로 분리하여 코드 배열로 반환
        private string[] ParseCodes(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) // 빈 문자열 검사
            {
                return Array.Empty<string>(); // 빈 배열 반환
            }
            
            string[] separators = new string[] { ",", " ", "\t" }; // 구분자 배열
            string[] rawCodes = value.Split(separators, StringSplitOptions.RemoveEmptyEntries); // 구분자로 분리
            
            return rawCodes; // 분리된 코드 배열 반환
        }

        // 코드 배열을 검증하고 UI에 반영하는 메서드
        public void ValidateAndHighlight(string[] codes)
        {
            if (codeValidator == null) // 코드 검증기가 없을 때
            {
                Debug.LogError("[CodeInputHighlighter] CodeValidator가 없습니다!");
                return;
            }
            
            ClearCodeList(); // 기존 코드 리스트 초기화
            ClearErrorMessage(); // 기존 오류 메시지 초기화

            CodeValidationResult[] results = codeValidator.ValidateCodes(codes); // 코드 검증

            UpdateCodeListUI(results); // 코드 리스트 UI 업데이트
            UpdateErrorMessage(results); // 오류 메시지 업데이트
        }

        // 현재 표시된 코드 아이템들을 모두 제거하는 메서드
        private void ClearCodeList()
        {
            foreach (GameObject item in currentCodeItems) // 현재 아이템들 순회
            {
                if (item != null) // null 체크
                {
                    DestroyImmediate(item); // 즉시 제거
                }
            }
            
            currentCodeItems.Clear(); // 리스트 초기화
        }

        private void ClearErrorMessage() // 오류 메시지 초기화 메서드
        {
            if (errorMessageText != null) // 오류 메시지 텍스트가 있을 때
            {
                errorMessageText.text = ""; // 텍스트 초기화
            }
        }

        // 코드 리스트 UI 업데이트 메서드
        private void UpdateCodeListUI(CodeValidationResult[] results)
        {
            if (codeContainer == null || itemPrefab == null) // 컨테이너나 프리팹이 없을 때
            {
                return; // 아무것도 하지 않음
            }

            foreach (CodeValidationResult result in results) // 각 검증 결과 순회
            {
                GameObject item = Instantiate(itemPrefab, codeContainer); // 프리팹 인스턴스화
                currentCodeItems.Add(item); // 현재 아이템 리스트에 추가

                TextMeshProUGUI codeText = item.GetComponentInChildren<TextMeshProUGUI>(); // 자식 텍스트 컴포넌트 가져오기
                if (codeText != null) // 텍스트 컴포넌트가 있을 때
                {
                    codeText.text = result.OriginalCode; // 원본 코드 설정

                    if (result.IsValid) // 유효한 경우
                    {
                        codeText.color = validColor; // 초록색
                    }
                    else // 유효하지 않은 경우
                    {
                        codeText.color = invalidColor; // 빨간색
                    }
                }
                
                Image background = item.GetComponent<Image>(); // 배경 이미지 컴포넌트 가져오기
                if (background != null && !result.IsValid) // 배경 이미지가 있고 유효하지 않은 경우
                {
                    Color backgroundColor = invalidColor; // 빨간색
                    backgroundColor.a = 0.3f; // 반투명
                    background.color = backgroundColor; // 배경 색상 설정
                }
            }
            
            Debug.Log($"[CodeInputHighlighter] {results.Length}개 코드 UI 업데이트 완료");
        }

        // 오류 메시지 업데이트 메서드
        private void UpdateErrorMessage(CodeValidationResult[] results)
        {
            if (errorMessageText == null) // 오류 메시지 텍스트가 없을 때
            {
                return; // 아무것도 하지 않음
            }
            
            List<string> errorMessages = new List<string>(); // 오류 메시지 리스트 초기화
            
            // 유효하지 않은 코드들의 오류 메시지 수집
            foreach (CodeValidationResult result in results)
            {
                if (!result.IsValid) // 유효하지 않은 경우
                {
                    errorMessages.Add(result.ErrorMessage); // 오류 메시지 추가
                }
            }

            // 오류 메시지 표시
            if (errorMessages.Count > 0)
            {
                errorMessageText.text = string.Join("\n", errorMessages); // 줄바꿈으로 연결
                errorMessageText.color = invalidColor; // 빨간색
            }
            else
            {
                errorMessageText.text = ""; // 오류 메시지 초기화
            }
        }

        // 외부에서 강제로 검증 트리거 (예: 버튼 클릭 시)
        public void TriggerValidation()
        {
            if (codeInputField != null) // 입력 필드가 있을 때
            {
                OnInputChanged(codeInputField.text); // 현재 텍스트로 검증 트리거
            }
        }
    }
}