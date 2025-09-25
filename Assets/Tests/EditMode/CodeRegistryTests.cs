using NUnit.Framework;
using UnityEngine;
using Core;  // 기존 Core namespace
using Data;  // 기존 ErrorCode

namespace Tests.EditMode  // 기존 Tests namespace
{
    public class CodeRegistryTests
    {
        private GameObject testObject;
        private CodeRegistry codeRegistry;
        private CodeValidator codeValidator;
        
        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestCodeRegistry");
            
            codeRegistry = testObject.AddComponent<CodeRegistry>();
            codeValidator = testObject.AddComponent<CodeValidator>();
            
            // 테스트용 등록 코드 설정
            var field = typeof(CodeRegistry).GetField("registeredCodes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(codeRegistry, new [] { "D20", "A15", "B03", "C10" });
            
            codeRegistry.Start();
            codeValidator.Start();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }
        
        #region AC-1 검증
        
        [Test]
        [Description("AC-1: D20, A15 입력 시 2개 코드가 올바르게 파싱되어 리스트로 표시")]
        public void AC1_TwoValidCodes_ParsedCorrectly()
        {
            // Arrange
            string[] inputCodes = { "D20", "A15" };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(inputCodes);
            
            // Assert
            Assert.AreEqual(2, results.Length, "2개 코드가 파싱되어야 함");
            
            Assert.IsTrue(results[0].IsValid, "D20은 유효한 코드여야 함");
            Assert.AreEqual("D20", results[0].NormalizedCode, "정규화된 코드가 D20이어야 함");
            
            Assert.IsTrue(results[1].IsValid, "A15는 유효한 코드여야 함");
            Assert.AreEqual("A15", results[1].NormalizedCode, "정규화된 코드가 A15여야 함");
        }
        
        [Test]
        public void AC1_CodesWithSpacesAndCommas_ParsedCorrectly()
        {
            // Arrange: 공백과 쉼표가 섞인 입력 (기존 CodeNormalizer 기능 활용)
            string[] inputCodes = { " D20 ", "A15,", " B03" };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(inputCodes);
            
            // Assert
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual("D20", results[0].NormalizedCode);
            Assert.AreEqual("A15", results[1].NormalizedCode);
            Assert.AreEqual("B03", results[2].NormalizedCode);
        }
        
        #endregion
        
        #region AC-2 검증
        
        [Test]
        [Description("AC-2: 미등록 코드 Z99 입력 시 해당 코드 빨간색 하이라이트 + '등록되지 않은 코드입니다: Z99' 에러 메시지 표시")]
        public void AC2_UnregisteredCode_MarkedAsInvalid()
        {
            // Arrange
            string[] inputCodes = { "Z99" };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(inputCodes);
            
            // Assert
            Assert.AreEqual(1, results.Length);
            Assert.IsFalse(results[0].IsValid, "Z99는 등록되지 않은 코드이므로 무효해야 함");
            Assert.AreEqual(ErrorCode.INVALID_CODE, results[0].ErrorCode);
            Assert.AreEqual("등록되지 않은 코드입니다: Z99", results[0].ErrorMessage, "정확한 에러 메시지가 표시되어야 함");
        }
        
        [Test]
        public void AC2_MixedValidAndInvalidCodes_CorrectlyIdentified()
        {
            // Arrange: 유효한 코드와 무효한 코드 혼합
            string[] inputCodes = { "D20", "Z99", "A15", "X88" };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(inputCodes);
            
            // Assert
            Assert.AreEqual(4, results.Length);
            
            // 유효한 코드들
            Assert.IsTrue(results[0].IsValid, "D20은 유효해야 함");
            Assert.IsTrue(results[2].IsValid, "A15는 유효해야 함");
            
            // 무효한 코드들  
            Assert.IsFalse(results[1].IsValid, "Z99는 무효해야 함");
            Assert.IsFalse(results[3].IsValid, "X88은 무효해야 함");
            
            // 에러 메시지 검증
            Assert.AreEqual("등록되지 않은 코드입니다: Z99", results[1].ErrorMessage);
            Assert.AreEqual("등록되지 않은 코드입니다: X88", results[3].ErrorMessage);
            
            // ErrorCode 검증 (기존 Data.ErrorCode 사용)
            Assert.AreEqual(ErrorCode.INVALID_CODE, results[1].ErrorCode);
            Assert.AreEqual(ErrorCode.INVALID_CODE, results[3].ErrorCode);
        }
        
        #endregion
        
        #region 기존 CodeNormalizer 통합 테스트
        
        [Test]
        public void Integration_CodeNormalizerAndValidator_WorkTogether()
        {
            // Arrange: CodeNormalizer가 처리할 수 있는 형식의 코드
            string[] inputCodes = { "d-20", " A15 ", "b_03" };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(inputCodes);
            
            // Assert: 정규화 + 검증이 모두 성공해야 함
            Assert.AreEqual(3, results.Length);
            
            Assert.IsTrue(results[0].IsValid);
            Assert.AreEqual("D20", results[0].NormalizedCode, "d-20이 D20으로 정규화되어야 함");
            
            Assert.IsTrue(results[1].IsValid);
            Assert.AreEqual("A15", results[1].NormalizedCode, " A15 가 A15로 정규화되어야 함");
            
            Assert.IsTrue(results[2].IsValid);
            Assert.AreEqual("B03", results[2].NormalizedCode, "b_03이 B03으로 정규화되어야 함");
        }
        
        [Test]
        public void Integration_InvalidFormatRejectedByNormalizer()
        {
            // Arrange: CodeNormalizer가 거부할 형식들
            string[] invalidFormats = { "ABC", "123", "A1234" };
            
            foreach (string invalidCode in invalidFormats)
            {
                // Act
                CodeValidationResult result = codeValidator.ValidateSingleCode(invalidCode);
                
                // Assert
                Assert.IsFalse(result.IsValid, $"{invalidCode}는 형식 오류로 무효해야 함");
                Assert.AreEqual(ErrorCode.INVALID_CODE, result.ErrorCode);
                Assert.That(result.ErrorMessage, Contains.Substring("잘못된 코드 형식"));
            }
        }
        
        #endregion
        
        #region 추가 검증 테스트
        
        [Test]
        public void CodeRegistry_ValidCodes_RecognizedCorrectly()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(codeRegistry.IsValidCode("D20"));
            Assert.IsTrue(codeRegistry.IsValidCode("A15"));
            Assert.IsTrue(codeRegistry.IsValidCode("B03"));
            Assert.IsTrue(codeRegistry.IsValidCode("C10"));
            
            Assert.IsFalse(codeRegistry.IsValidCode("Z99"));
            Assert.IsFalse(codeRegistry.IsValidCode(""));
            Assert.IsFalse(codeRegistry.IsValidCode(null));
        }
        
        [Test]
        public void CodeValidator_EmptyInput_ReturnsEmptyArray()
        {
            // Arrange
            string[] emptyCodes = { };
            
            // Act
            CodeValidationResult[] results = codeValidator.ValidateCodes(emptyCodes);
            
            // Assert
            Assert.AreEqual(0, results.Length);
        }
        
        [Test]
        public void ErrorCode_Integration_UsesExistingErrorMessages()
        {
            // Arrange
            string invalidCode = "Z99";
            
            // Act
            CodeValidationResult result = codeValidator.ValidateSingleCode(invalidCode);
            
            // Assert: 기존 ErrorCode enum 활용 확인
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(ErrorCode.INVALID_CODE, result.ErrorCode);
            
            // 기존 ErrorCodeExtensions 활용 가능 (선택사항)
            string expectedMessage = ErrorCode.INVALID_CODE.ToMessage();
            Assert.IsNotNull(expectedMessage);
        }
        
        #endregion
    }
}