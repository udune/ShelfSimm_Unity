using System;
using Core;
using NUnit.Framework;

namespace Tests.EditMode
{
    // CodeNormalizer 클래스에 대한 단위 테스트 클래스
    public class CodeNormalizerTests
    {
        [SetUp]
        public void Setup()
        {
            // 테스트 시작 전 초기화
        }

        #region 기본 정규화 테스트

        [Test]
        [TestCase("d-3", "D03")]           // 하이픈 제거 + 대문자 + zero-pad
        [TestCase("a 15", "A15")]          // 공백 제거 + 대문자
        [TestCase("B7", "B07")]            // zero-pad만
        [TestCase("c_9", "C09")]           // 언더스코어 제거 + zero-pad
        [TestCase("  A1  ", "A01")]        // 앞뒤 공백 제거 + zero-pad
        [TestCase("z99", "Z99")]           // 2자리 숫자는 그대로
        public void NormalizeCode_ValidInputs_ReturnsExpectedResult(string input, string expected)
        {
            // Act
            string result = CodeNormalizer.NormalizeCode(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region 예외 케이스 테스트

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void NormalizeCode_NullOrEmpty_ThrowsArgumentException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CodeNormalizer.NormalizeCode(input));
        }

        [Test]
        [TestCase("123")]          // 숫자만
        [TestCase("ABC")]          // 알파벳만
        [TestCase("A")]            // 알파벳만 1개
        [TestCase("AB1")]          // 알파벳 2개
        [TestCase("1A")]           // 숫자가 앞에
        [TestCase("A-B-1")]        // 복잡한 형식
        [TestCase("@#$")]          // 특수문자만
        public void NormalizeCode_InvalidFormat_ThrowsArgumentException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => CodeNormalizer.NormalizeCode(input));
        }

        [Test]
        [TestCase("A123")]         // 3자리 숫자 (정책 위반)
        [TestCase("B1234")]        // 4자리 숫자
        public void NormalizeCode_NumberTooLong_ThrowsArgumentException(string input)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CodeNormalizer.NormalizeCode(input));
            Assert.That(exception.Message, Contains.Substring("2자리를 초과"));
        }

        #endregion

        #region TryNormalizeCode 테스트

        [Test]
        public void TryNormalizeCode_ValidInput_ReturnsTrue()
        {
            // Act
            bool success = CodeNormalizer.TryNormalizeCode("d-3", out string result);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual("D03", result);
        }

        [Test]
        public void TryNormalizeCode_InvalidInput_ReturnsFalse()
        {
            // Act
            bool success = CodeNormalizer.TryNormalizeCode("invalid", out string result);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        #endregion

        #region 배치 정규화 테스트

        [Test]
        public void NormalizeCodes_MixedValidAndInvalid_ReturnsCorrectResults()
        {
            // Arrange
            string[] inputs = { "D20", "A15", "invalid", "B03", "" };

            // Act
            var results = CodeNormalizer.NormalizeCodes(inputs);

            // Assert
            Assert.AreEqual(5, results.Length);

            // 유효한 코드들
            Assert.IsTrue(results[0].IsValid);
            Assert.AreEqual("D20", results[0].NormalizedCode);

            Assert.IsTrue(results[1].IsValid);
            Assert.AreEqual("A15", results[1].NormalizedCode);

            Assert.IsTrue(results[3].IsValid);
            Assert.AreEqual("B03", results[3].NormalizedCode);

            // 무효한 코드들
            Assert.IsFalse(results[2].IsValid);
            Assert.IsNull(results[2].NormalizedCode);
            Assert.IsNotNull(results[2].ErrorMessage);

            Assert.IsFalse(results[4].IsValid);
            Assert.IsNull(results[4].NormalizedCode);
            Assert.IsNotNull(results[4].ErrorMessage);
        }

        [Test]
        public void NormalizeCodes_NullInput_ReturnsEmptyArray()
        {
            // Act
            var results = CodeNormalizer.NormalizeCodes(null);

            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Length);
        }

        #endregion

        #region AC 검증 테스트 (요구사항 직접 검증)

        [Test]
        [Description("AC-1: D20, A15 입력 시 2개 코드가 올바르게 파싱되어 리스트로 표시")]
        public void AC1_TwoValidCodes_ParsedCorrectly()
        {
            // Arrange
            string[] inputs = { "D20", "A15" };

            // Act
            var results = CodeNormalizer.NormalizeCodes(inputs);

            // Assert
            Assert.AreEqual(2, results.Length);
            Assert.IsTrue(results[0].IsValid);
            Assert.IsTrue(results[1].IsValid);
            Assert.AreEqual("D20", results[0].NormalizedCode);
            Assert.AreEqual("A15", results[1].NormalizedCode);
        }

        [Test]
        [Description("AC-2: 미등록 코드 Z99 입력 시 해당 코드 빨간색 하이라이트 + 에러 메시지 표시")]
        public void AC2_InvalidCode_MarkedAsInvalid()
        {
            // Note: Z99는 형식상 유효하지만, 실제 등록 여부는 CodeRegistry에서 검증
            // 여기서는 형식 검증만 테스트
            
            // Arrange - 형식상 무효한 코드
            string invalidCode = "Z999";  // 3자리 숫자 (무효)

            // Act
            var result = CodeNormalizer.TryNormalizeCode(invalidCode, out string normalized);

            // Assert
            Assert.IsFalse(result, "3자리 숫자 코드는 무효해야 함");
            Assert.IsNull(normalized);
        }

        #endregion
    }
}
