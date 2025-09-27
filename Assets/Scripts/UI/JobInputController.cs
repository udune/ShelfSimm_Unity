using System;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 작업 입력 컨트롤러 클래스
    public class JobInputController : MonoBehaviour
    {
        [Header("UI")] 
        [SerializeField] private TMP_InputField cellCodesInput; // 쉼표로 구분된 칸 코드 입력 필드
        [SerializeField] private TMP_Dropdown actionTypeDropdown; // 작업 유형 드롭다운 (PUT, PICK)
        [SerializeField] private TMP_Dropdown bookDropdown; // 도서 선택 드롭다운
        [SerializeField] private TMP_InputField quantityInput; // 작업 수량 입력 필드
        [SerializeField] private Button executeButton; // 실행 버튼
        [SerializeField] private TextMeshProUGUI statusText; // 상태 표시 텍스트
        [SerializeField] private Slider completenessSlider; // 입력 완성도 슬라이더

        [Header("Error")] 
        [SerializeField] private GameObject errorPanel; // 에러 메시지 패널
        [SerializeField] private TextMeshProUGUI errorText; // 에러 메시지 텍스트

        [Header("Setting")] 
        [SerializeField] private Color validColor = Color.white; // 유효한 입력 색상
        [SerializeField] private Color invalidColor = Color.red; // 유효하지 않은 입력 색상

        private JobInputData currentJobInput = new JobInputData(); // 현재 작업 입력 데이터
        private bool isInitialized = false; // 초기화 여부

        public Action<JobInputData> OnValidInputChanged; // 유효한 입력이 변경될 때 호출되는 이벤트
        public Action<JobInputData> OnExecuteRequested; // 실행 요청 이벤트

        private void Start()
        {
            Init(); // 초기화
        }

        private void Init()
        {
            if (actionTypeDropdown != null) // 작업 유형 드롭다운이 있을 때
            {
                actionTypeDropdown.ClearOptions(); // 기존 옵션 제거
                actionTypeDropdown.AddOptions(new List<string> { "PUT", "PICK" }); // 옵션 추가
                actionTypeDropdown.value = 0; // 기본값 설정
                actionTypeDropdown.onValueChanged.AddListener(OnActionTypeChanged); // 값 변경 이벤트에 리스너 추가
            }

            if (quantityInput != null) // 수량 입력 필드가 있을 때
            {
                quantityInput.text = "1"; // 기본값 설정
                quantityInput.contentType = TMP_InputField.ContentType.IntegerNumber; // 정수만 입력 가능
                quantityInput.onValueChanged.AddListener(OnQuantityChanged); // 값 변경 이벤트에 리스너 추가
                quantityInput.onEndEdit.AddListener(OnQuantityEndEdit); // 편집 종료 이벤트에 리스너 추가
            }

            if (cellCodesInput != null) // 칸 코드 입력 필드가 있을 때
            {
                cellCodesInput.onValueChanged.AddListener(OnCellCodesChanged); // 값 변경 이벤트에 리스너 추가
                cellCodesInput.placeholder.GetComponent<TextMeshProUGUI>().text = "예: D20, A15, B03"; // 플레이스홀더 텍스트 설정
            }

            if (bookDropdown != null) // 도서 선택 드롭다운이 있을 때
            {
                bookDropdown.onValueChanged.AddListener(OnBookChanged); // 값 변경 이벤트에 리스너 추가
                LoadDummyBooks(); // 더미 도서 목록 로드
            }

            if (executeButton != null) // 실행 버튼이 있을 때
            {
                executeButton.onClick.AddListener(OnExecuteClicked); // 클릭 이벤트에 리스너 추가
            }

            currentJobInput.quantity = 1; // 기본 수량 설정
            isInitialized = true; // 초기화 완료
            UpdateUI(); // UI 업데이트
        }

        private void LoadDummyBooks() // 더미 도서 목록 로드
        {
            if (bookDropdown == null) // 도서 선택 드롭다운이 없을 때
            {
                return; // 종료
            }
            
            bookDropdown.ClearOptions(); // 기존 옵션 제거
            bookDropdown.AddOptions(new List<string> // 더미 도서 목록 추가
            {
                "도서를 선택하세요",
                "The Great Gatsby",
                "1984",
                "To Kill a Mockingbird",
                "Pride and Prejudice"
            });
            bookDropdown.value = 0; // 기본값 설정
        }

        private void OnCellCodesChanged(string input) // 칸 코드 입력 필드 값 변경 시 호출되는 메서드
        {
            if (!isInitialized) // 초기화되지 않았을 때
            {
                return; // 종료
            }
            
            currentJobInput.cellCodesText = input; // 입력 텍스트 저장
            ParseCellCodes(input); // 칸 코드 파싱
            UpdateUI(); // UI 업데이트
        }

        private void ParseCellCodes(string input) // 칸 코드 파싱 메서드
        {
            currentJobInput.parsedCodes.Clear(); // 기존 파싱된 코드 초기화
            currentJobInput.invalidCodes.Clear(); // 기존 유효하지 않은 코드 초기화
            
            if (string.IsNullOrWhiteSpace(input)) // 입력이 비어있을 때
            {
                return; // 종료
            }
            
            string[] codes = input.Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries); // 쉼표, 공백으로 분리

            foreach (string code in codes) // 각 코드에 대해
            {
                string trimmed = code.Trim(); // 앞뒤 공백 제거
                if (string.IsNullOrEmpty(trimmed)) // 빈 문자열이면
                {
                    continue; // 건너뜀
                }

                if (CodeNormalizer.TryNormalizeCode(trimmed, out string normalized)) // 코드 정규화 시도
                {
                    currentJobInput.parsedCodes.Add(normalized); // 정규화된 코드 추가
                }
                else
                {
                    currentJobInput.invalidCodes.Add(trimmed); // 유효하지 않은 코드 추가
                }
            }
        }

        private void OnActionTypeChanged(int value) // 작업 유형 드롭다운 값 변경 시 호출되는 메서드
        {
            if (!isInitialized) // 초기화되지 않았을 때
            {
                return; // 종료
            }

            currentJobInput.actionType = (JobAction)value; // 선택된 작업 유형 저장
            UpdateUI(); // UI 업데이트
        }

        private void OnBookChanged(int value) // 도서 선택 드롭다운 값 변경 시 호출되는 메서드
        {
            if (!isInitialized) // 초기화되지 않았을 때
            {
                return; // 종료
            }
            
            if (value > 0 && bookDropdown != null) // 유효한 도서가 선택되었을 때
            {
                currentJobInput.bookId = $"book_{value}"; // 더미 도서 ID 설정
            }
            else
            {
                currentJobInput.bookId = ""; // 도서 선택 해제
            }
            
            UpdateUI(); // UI 업데이트
        }

        private void OnQuantityChanged(string input) // 수량 입력 필드 값 변경 시 호출되는 메서드
        {
            if (!isInitialized) // 초기화되지 않았을 때
            {
                return; // 종료
            }

            if (string.IsNullOrWhiteSpace(input)) // 입력이 비어있을 때
            {
                currentJobInput.quantity = 0; // 수량 0으로 설정
            }
            else if (int.TryParse(input, out int value)) // 정수로 변환 시도
            {
                currentJobInput.quantity = value; // 변환 성공 시 수량 저장
            }
            
            UpdateUI(); // UI 업데이트
        }

        private void OnQuantityEndEdit(string input) // 수량 입력 필드 편집 종료 시 호출되는 메서드
        {
            if (!isInitialized || quantityInput == null) // 초기화되지 않았거나 수량 입력 필드가 없을 때
            {
                return; // 종료
            }
            
            int correctedQuantity = InputValidator.CorrectQuantity(currentJobInput.quantity); // 수량 보정

            if (correctedQuantity != currentJobInput.quantity) // 보정된 수량이 다를 때
            {
                currentJobInput.quantity = correctedQuantity; // 보정된 수량 저장
                quantityInput.text = correctedQuantity.ToString(); // 입력 필드에 보정된 수량 표시
                
                ShowTemporaryStatus($"수량이 {correctedQuantity}로 보정되었습니다.", 2f); // 임시 상태 메시지 표시
            }

            UpdateUI(); // UI 업데이트
        }

        private void OnExecuteClicked() // 실행 버튼 클릭 시 호출되는 메서드
        {
            var validation = InputValidator.ValidateJobInput(currentJobInput); // 입력 검증

            if (validation.IsValid) // 입력이 유효할 때
            {
                currentJobInput.quantity = validation.CorrectedQuantity; // 보정된 수량 적용
                
                OnExecuteRequested?.Invoke(currentJobInput); // 실행 요청 이벤트 호출
                
                Debug.Log($"[JobInputController] 작업 실행 요청: {currentJobInput}");
            }
            else
            {
                Debug.LogWarning($"[JobInputController] 입력 검증 실패: {string.Join(", ", validation.ErrorMessages)}");
            }
        }

        private void UpdateUI() // UI 업데이트 메서드
        {
            if (!isInitialized) // 초기화되지 않았을 때
            {
                return; // 종료
            }
            
            var validation = InputValidator.ValidateJobInput(currentJobInput); // 입력 검증
            float completeness = InputValidator.GetInputCompleteness(currentJobInput); // 입력 완성도 계산

            if (executeButton != null) // 실행 버튼이 있을 때
            {
                bool isEnable = InputValidator.IsEnableExecuteButton(currentJobInput); // 실행 버튼 활성화 여부 판단
                executeButton.interactable = isEnable; // 실행 버튼 활성화/비활성화 설정

                var colors = executeButton.colors; // 버튼 색상 가져오기
                colors.normalColor = isEnable ? Color.green : Color.gray; // 활성화 시 초록색, 비활성화 시 회색
                executeButton.colors = colors; // 버튼 색상 적용
            }

            if (statusText != null) // 상태 표시 텍스트가 있을 때
            {
                if (validation.IsValid) // 입력이 유효할 때
                {
                    statusText.text = "모든 입력이 유효합니다."; // 상태 메시지 설정
                    statusText.color = validColor; // 유효한 색상 적용
                }
                else
                {
                    statusText.text = "입력에 오류가 있습니다."; // 상태 메시지 설정
                    statusText.color = invalidColor; // 유효하지 않은 색상 적용
                }
            }

            if (completenessSlider != null) // 입력 완성도 슬라이더가 있을 때
            {
                completenessSlider.value = completeness; // 슬라이더 값 설정
            }
            
            UpdateErrorDisplay(validation.ErrorMessages); // 에러 메시지 업데이트

            if (cellCodesInput != null) // 칸 코드 입력 필드가 있을 때
            {
                var image = cellCodesInput.GetComponent<Image>(); // 입력 필드의 이미지 컴포넌트 가져오기
                if (image != null) // 이미지 컴포넌트가 있을 때
                {
                    if (currentJobInput.invalidCodes != null && currentJobInput.invalidCodes.Count > 0) // 유효하지 않은 코드가 있을 때
                    {
                        image.color = invalidColor; // 유효하지 않은 색상 적용
                    }
                    else
                    {
                        image.color = validColor; // 유효한 색상 적용
                    }
                }
            }

            if (validation.IsValid) // 입력이 유효할 때
            {
                OnValidInputChanged?.Invoke(currentJobInput); // 유효한 입력 변경 이벤트 호출
            }
        }
        
        private void UpdateErrorDisplay(List<string> errors) // 에러 메시지 업데이트 메서드
        {
            bool hasErrors = errors != null && errors.Count > 0; // 에러 존재 여부 판단

            if (errorPanel != null) // 에러 패널이 있을 때
            {
                errorPanel.SetActive(hasErrors); // 에러 패널 활성화/비활성화 설정
            }
            
            if (errorText != null && hasErrors) // 에러 텍스트가 있을 때와 에러가 있을 때
            {
                errorText.text = string.Join("\n", errors); // 에러 메시지 줄바꿈으로 연결
                errorText.color = invalidColor; // 유효하지 않은 색상 적용
            }
        }

        private void ShowTemporaryStatus(string message, float duration) // 임시 상태 메시지 표시 메서드
        {
            Debug.Log($"[JobInputController] {message}");

            if (statusText != null) // 상태 표시 텍스트가 있을 때
            {
                string originalText = statusText.text; // 원래 텍스트 저장
                statusText.text = message; // 임시 메시지 설정
                statusText.color = Color.yellow; // 임시 메시지 색상 설정
                
                Invoke(nameof(RestoreStatusText), duration); // 일정 시간 후 원래 텍스트 복원
            }
        }

        private void RestoreStatusText() // 상태 텍스트 복원 메서드
        {
            UpdateUI(); // UI 업데이트를 통해 원래 상태 복원
        }
        
        public void SetJobInput(JobInputData jobInput) // 외부에서 작업 입력 설정 메서드
        {
            if (jobInput == null) // 입력이 null일 때
            {
                return; // 종료
            }
            
            currentJobInput = jobInput; // 현재 작업 입력 데이터 설정

            if (cellCodesInput != null) // 칸 코드 입력 필드가 있을 때
            {
                cellCodesInput.text = jobInput.cellCodesText; // 입력 텍스트 설정
            }
            
            if (actionTypeDropdown != null) // 작업 유형 드롭다운이 있을 때
            {
                actionTypeDropdown.value = (int)jobInput.actionType; // 선택된 작업 유형 설정
            }

            if (quantityInput != null) // 수량 입력 필드가 있을 때
            {
                quantityInput.text = jobInput.quantity.ToString(); // 수량 텍스트 설정
            }

            UpdateUI(); // UI 업데이트
        }
        
        public JobInputData GetCurrentJobInput() // 현재 작업 입력 데이터 반환 메서드
        {
            return currentJobInput; // 현재 작업 입력 데이터 반환
        }

        public void ResetInput() // 입력 초기화 메서드
        {
            currentJobInput = new JobInputData { quantity = 1 }; // 새로운 작업 입력 데이터 생성 및 기본 수량 설정

            if (cellCodesInput != null) // 칸 코드 입력 필드가 있을 때
            {
                cellCodesInput.text = ""; // 입력 텍스트 초기화
            }

            if (actionTypeDropdown != null) // 작업 유형 드롭다운이 있을 때
            {
                actionTypeDropdown.value = 0; // 기본값 설정
            }

            if (bookDropdown != null) // 도서 선택 드롭다운이 있을 때
            {
                bookDropdown.value = 0; // 기본값 설정
            }

            if (quantityInput != null) // 수량 입력 필드가 있을 때
            {
                quantityInput.text = "1"; // 기본값 설정
            }

            UpdateUI(); // UI 업데이트
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void RunTests()
        {
            Debug.Log("=== JobInputController 테스트 시작 ===");
            InputValidator.TestHelper.TestQuantityCorrection();
            InputValidator.TestHelper.TestInputValidation();
            Debug.Log("=== JobInputController 테스트 종료 ===");
        }
    }
}