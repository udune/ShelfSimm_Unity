using System.Collections.Generic;
using Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode.Input
{
    /// <summary>
    /// InputValidator 단위 테스트
    /// T-104: 기본값/경계값(>0) 보정 + 입력 완성도 검증 테스트
    /// </summary>
    [TestFixture]
    public class InputValidatorTests
    {
        #region 수량 보정 테스트 (AC-3)

        [Test]
        [Description("AC-3: 빈 수량 또는 0 이하 입력 시 기본값 1로 자동 보정")]
        public void CorrectQuantity_ZeroOrNegative_ReturnsDefaultValue()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(1, InputValidator.CorrectQuantity(0), "0은 기본값 1로 보정되어야 함");
            Assert.AreEqual(1, InputValidator.CorrectQuantity(-1), "-1은 기본값 1로 보정되어야 함");
            Assert.AreEqual(1, InputValidator.CorrectQuantity(-100), "-100은 기본값 1로 보정되어야 함");
        }

        [Test]
        public void CorrectQuantity_ValidRange_ReturnsOriginalValue()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(1, InputValidator.CorrectQuantity(1), "1은 그대로 반환되어야 함");
            Assert.AreEqual(5, InputValidator.CorrectQuantity(5), "5는 그대로 반환되어야 함");
            Assert.AreEqual(999, InputValidator.CorrectQuantity(999), "999는 그대로 반환되어야 함");
        }

        [Test]
        public void CorrectQuantity_ExceedsMaximum_ReturnsMaxValue()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(999, InputValidator.CorrectQuantity(1000), "1000은 최대값 999로 제한되어야 함");
            Assert.AreEqual(999, InputValidator.CorrectQuantity(9999), "9999는 최대값 999로 제한되어야 함");
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(-5, 1)]
        [TestCase(1, 1)]
        [TestCase(50, 50)]
        [TestCase(999, 999)]
        [TestCase(1000, 999)]
        [TestCase(9999, 999)]
        [Description("다양한 수량 값에 대한 보정 결과 검증")]
        public void CorrectQuantity_VariousValues_ReturnsExpectedResult(int input, int expected)
        {
            // Act
            int result = InputValidator.CorrectQuantity(input);

            // Assert
            Assert.AreEqual(expected, result, $"입력값 {input}에 대한 보정 결과가 {expected}이어야 함");
        }

        #endregion

        #region 입력 검증 테스트 (DoD)

        [Test]
        [Description("DoD: 필수 필드 미입력 시 실행 버튼 비활성화 - 완전한 입력")]
        public void ValidateJobInput_CompleteInput_ReturnsValid()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20, A15",
                parsedCodes = new List<string> { "D20", "A15" },
                invalidCodes = new List<string>(),
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 2
            };

            // Act
            var result = InputValidator.ValidateJobInput(jobInput);

            // Assert
            Assert.IsTrue(result.IsValid, "완전한 입력은 유효해야 함");
            Assert.IsTrue(result.HasCellCodes, "칸 코드가 있어야 함");
            Assert.IsTrue(result.HasValidBook, "유효한 도서가 선택되어야 함");
            Assert.IsTrue(result.HasValidQuantity, "유효한 수량이어야 함");
            Assert.AreEqual(2, result.CorrectedQuantity, "보정된 수량이 일치해야 함");
            Assert.AreEqual(0, result.ErrorMessages.Count, "에러 메시지가 없어야 함");
        }

        [Test]
        [Description("DoD: 칸 코드 누락 시 검증 실패")]
        public void ValidateJobInput_MissingCellCodes_ReturnsInvalid()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "",  // 칸 코드 누락
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 1
            };

            // Act
            var result = InputValidator.ValidateJobInput(jobInput);

            // Assert
            Assert.IsFalse(result.IsValid, "칸 코드가 없으면 무효해야 함");
            Assert.IsFalse(result.HasCellCodes, "칸 코드가 없다고 표시되어야 함");
            Assert.IsTrue(result.ErrorMessages.Count > 0, "에러 메시지가 있어야 함");
            Assert.IsTrue(result.ErrorMessages.Exists(msg => msg.Contains("칸 코드")), "칸 코드 관련 에러 메시지가 있어야 함");
        }

        [Test]
        [Description("DoD: 도서 선택 누락 시 검증 실패")]
        public void ValidateJobInput_MissingBook_ReturnsInvalid()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                bookId = "",  // 도서 선택 누락
                quantity = 1
            };

            // Act
            var result = InputValidator.ValidateJobInput(jobInput);

            // Assert
            Assert.IsFalse(result.IsValid, "도서가 선택되지 않으면 무효해야 함");
            Assert.IsTrue(result.HasCellCodes, "칸 코드는 있어야 함");
            Assert.IsFalse(result.HasValidBook, "도서가 없다고 표시되어야 함");
            Assert.IsTrue(result.ErrorMessages.Count > 0, "에러 메시지가 있어야 함");
            Assert.IsTrue(result.ErrorMessages.Exists(msg => msg.Contains("도서")), "도서 관련 에러 메시지가 있어야 함");
        }

        [Test]
        [Description("무효한 코드가 포함된 경우 검증 실패")]
        public void ValidateJobInput_WithInvalidCodes_ReturnsInvalid()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20, Z99",
                parsedCodes = new List<string> { "D20" },
                invalidCodes = new List<string> { "Z99" },  // 무효한 코드 포함
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 1
            };

            // Act
            var result = InputValidator.ValidateJobInput(jobInput);

            // Assert
            Assert.IsFalse(result.IsValid, "무효한 코드가 있으면 무효해야 함");
            Assert.IsTrue(result.ErrorMessages.Count > 0, "에러 메시지가 있어야 함");
            Assert.IsTrue(result.ErrorMessages.Exists(msg => msg.Contains("Z99")), "무효한 코드가 에러 메시지에 포함되어야 함");
        }

        [Test]
        [Description("수량 보정이 검증에 반영되는지 테스트")]
        public void ValidateJobInput_QuantityCorrection_ReflectedInResult()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = -5  // 음수 수량 (보정 필요)
            };

            // Act
            var result = InputValidator.ValidateJobInput(jobInput);

            // Assert
            Assert.AreEqual(1, result.CorrectedQuantity, "음수 수량이 1로 보정되어야 함");
            // 수량 보정은 에러가 아니므로 여전히 유효할 수 있음 (다른 필드가 모두 채워졌다면)
        }

        [Test]
        [Description("null 입력에 대한 처리")]
        public void ValidateJobInput_NullInput_ReturnsInvalid()
        {
            // Act
            var result = InputValidator.ValidateJobInput(null);

            // Assert
            Assert.IsFalse(result.IsValid, "null 입력은 무효해야 함");
            Assert.IsTrue(result.ErrorMessages.Count > 0, "에러 메시지가 있어야 함");
        }

        #endregion

        #region 실행 버튼 활성화 테스트

        [Test]
        [Description("DoD: 완전한 입력 시 실행 버튼 활성화")]
        public void ShouldEnableExecuteButton_CompleteInput_ReturnsTrue()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 1
            };

            // Act
            bool shouldEnable = InputValidator.IsEnableExecuteButton(jobInput);

            // Assert
            Assert.IsTrue(shouldEnable, "완전한 입력 시 실행 버튼이 활성화되어야 함");
        }

        [Test]
        [Description("DoD: 불완전한 입력 시 실행 버튼 비활성화")]
        public void ShouldEnableExecuteButton_IncompleteInput_ReturnsFalse()
        {
            // Arrange
            var jobInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                bookId = "",  // 도서 선택 누락
                quantity = 1
            };

            // Act
            bool shouldEnable = InputValidator.IsEnableExecuteButton(jobInput);

            // Assert
            Assert.IsFalse(shouldEnable, "불완전한 입력 시 실행 버튼이 비활성화되어야 함");
        }

        #endregion

        #region 완성도 계산 테스트

        [Test]
        [Description("입력 완성도 계산 테스트")]
        public void GetInputCompleteness_VariousInputs_ReturnsCorrectPercentage()
        {
            // 완전한 입력 (100%)
            var completeInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 1
            };
            Assert.AreEqual(1.0f, InputValidator.GetInputCompleteness(completeInput), 0.01f, "완전한 입력은 100%여야 함");

            // 칸 코드만 입력 (50%)
            var partialInput = new JobInputData
            {
                cellCodesText = "D20",
                parsedCodes = new List<string> { "D20" },
                actionType = JobAction.PUT,
                quantity = 1
                // bookId 누락
            };
            Assert.AreEqual(0.7f, InputValidator.GetInputCompleteness(partialInput), 0.01f, "부분 입력은 70%여야 함");

            // 빈 입력 (20% - 수량은 항상 유효)
            var emptyInput = new JobInputData
            {
                quantity = 1
            };
            Assert.AreEqual(0.2f, InputValidator.GetInputCompleteness(emptyInput), 0.01f, "빈 입력은 20%여야 함");

            // null 입력 (0%)
            Assert.AreEqual(0.0f, InputValidator.GetInputCompleteness(null), 0.01f, "null 입력은 0%여야 함");
        }

        #endregion

        #region AC 검증 테스트

        [Test]
        [Description("AC-3: 빈 수량 또는 0 이하 입력 시 기본값 1로 자동 보정 - 통합 테스트")]
        public void AC3_QuantityCorrection_IntegrationTest()
        {
            // Arrange - 다양한 문제가 있는 수량들
            var testCases = new (int input, int expected)[]
            {
                (-10, 1),   // 음수 -> 1
                (0, 1),     // 0 -> 1  
                (1, 1),     // 1 -> 1 (변화없음)
                (50, 50),   // 정상값 -> 변화없음
                (1000, 999) // 초과값 -> 최대값
            };

            foreach (var (input, expected) in testCases)
            {
                var jobInput = new JobInputData
                {
                    cellCodesText = "D20",
                    parsedCodes = new List<string> { "D20" },
                    actionType = JobAction.PUT,
                    bookId = "book123",
                    quantity = input
                };

                // Act
                var result = InputValidator.ValidateJobInput(jobInput);

                // Assert
                Assert.AreEqual(expected, result.CorrectedQuantity, 
                    $"수량 {input}이 {expected}으로 보정되어야 함");
                
                Debug.Log($"[AC-3 검증] 입력: {input} -> 보정: {result.CorrectedQuantity}");
            }
        }

        [Test]
        [Description("DoD: 필수 필드 미입력 시 실행 버튼 비활성화 - 통합 테스트")]
        public void DoD_RequiredFields_IntegrationTest()
        {
            // 테스트 케이스: (설명, 입력데이터, 예상 활성화 상태)
            var testCases = new (string desc, JobInputData input, bool expectedEnabled)[]
            {
                ("모든 필드 완성", new JobInputData 
                {
                    cellCodesText = "D20",
                    parsedCodes = new List<string> { "D20" },
                    bookId = "book123",
                    quantity = 1
                }, true),
                
                ("칸 코드 누락", new JobInputData 
                {
                    cellCodesText = "",
                    bookId = "book123",
                    quantity = 1
                }, false),
                
                ("도서 선택 누락", new JobInputData 
                {
                    cellCodesText = "D20",
                    parsedCodes = new List<string> { "D20" },
                    bookId = "",
                    quantity = 1
                }, false),
                
                ("무효한 코드 포함", new JobInputData 
                {
                    cellCodesText = "INVALID",
                    parsedCodes = new List<string>(),
                    invalidCodes = new List<string> { "INVALID" },
                    bookId = "book123",
                    quantity = 1
                }, false)
            };

            foreach (var (desc, input, expectedEnabled) in testCases)
            {
                // Act
                bool actualEnabled = InputValidator.IsEnableExecuteButton(input);

                // Assert
                Assert.AreEqual(expectedEnabled, actualEnabled, $"{desc}: 실행 버튼 활성화 상태가 {expectedEnabled}이어야 함");
                
                Debug.Log($"[DoD 검증] {desc}: 예상={expectedEnabled}, 실제={actualEnabled}");
            }
        }

        #endregion

        #region 성능 및 경계값 테스트

        [Test]
        [Description("대량 코드 처리 성능 테스트")]
        public void ValidateJobInput_LargeCodeList_PerformsWell()
        {
            // Arrange - 대량의 코드 생성
            var codes = new List<string>();
            for (int i = 1; i <= 100; i++)
            {
                codes.Add($"A{i:D2}");
            }

            var jobInput = new JobInputData
            {
                cellCodesText = string.Join(", ", codes),
                parsedCodes = codes,
                actionType = JobAction.PUT,
                bookId = "book123",
                quantity = 1
            };

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = InputValidator.ValidateJobInput(jobInput);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result.IsValid, "대량 코드 처리도 유효해야 함");
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "100ms 이내에 처리되어야 함");
            
            Debug.Log($"[성능 테스트] 100개 코드 검증 시간: {stopwatch.ElapsedMilliseconds}ms");
        }

        [Test]
        [Description("극단적인 수량값 경계 테스트")]
        public void CorrectQuantity_ExtremeBoundaryValues_HandledCorrectly()
        {
            // Act & Assert
            Assert.AreEqual(1, InputValidator.CorrectQuantity(int.MinValue), "int.MinValue는 1로 보정되어야 함");
            Assert.AreEqual(999, InputValidator.CorrectQuantity(int.MaxValue), "int.MaxValue는 999로 제한되어야 함");
        }

        #endregion
    }
}