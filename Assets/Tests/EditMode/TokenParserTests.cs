using Core;
using NUnit.Framework;

namespace Tests.EditMode
{
    /// <summary>
    /// TokenParser 단위 테스트
    /// T-102 작업의 수용 기준 검증
    /// </summary>
    [TestFixture]
    public class TokenParserTests
    {
        #region 기본 파싱 테스트

        [Test]
        public void ParseTokens_쉼표구분자_정상파싱()
        {
            // Given: 쉼표로 구분된 코드들
            string input = "D20, A15, B03";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 3개의 토큰으로 분리됨
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("D20", result[0]);
            Assert.AreEqual("A15", result[1]);
            Assert.AreEqual("B03", result[2]);
        }

        [Test]
        public void ParseTokens_공백구분자_정상파싱()
        {
            // Given: 공백으로 구분된 코드들
            string input = "D20 A15 B03";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 3개의 토큰으로 분리됨
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("D20", result[0]);
            Assert.AreEqual("A15", result[1]);
            Assert.AreEqual("B03", result[2]);
        }

        [Test]
        public void ParseTokens_혼합구분자_정상파싱()
        {
            // Given: 쉼표와 공백 혼합 구분
            string input = "D20, A15 B03,C07";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 4개의 토큰으로 분리됨
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("D20", result[0]);
            Assert.AreEqual("A15", result[1]);
            Assert.AreEqual("B03", result[2]);
            Assert.AreEqual("C07", result[3]);
        }

        [Test]
        public void ParseTokens_연속공백_정상처리()
        {
            // Given: 연속된 공백과 쉼표
            string input = "D20  ,  ,  A15    B03";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 빈 토큰 제거되고 3개만 반환
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("D20", result[0]);
            Assert.AreEqual("A15", result[1]);
            Assert.AreEqual("B03", result[2]);
        }

        #endregion

        #region 경계값 테스트

        [Test]
        public void ParseTokens_빈입력_빈배열반환()
        {
            // Given: 빈 문자열
            string input = "";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 빈 배열 반환
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void ParseTokens_null입력_빈배열반환()
        {
            // Given: null 입력
            string input = null;
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 빈 배열 반환
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void ParseTokens_공백만입력_빈배열반환()
        {
            // Given: 공백과 쉼표만
            string input = "  , ,   ";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 빈 배열 반환
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void ParseTokens_단일토큰_정상파싱()
        {
            // Given: 하나의 코드만
            string input = "  D20  ";
            
            // When: 토큰 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 1개 토큰 반환 (앞뒤 공백 제거됨)
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("D20", result[0]);
        }

        #endregion

        #region 정규화 통합 테스트

        [Test]
        public void ParseAndNormalize_정상코드_정규화성공()
        {
            // Given: 대소문자 혼합, 패딩 필요한 코드들
            string input = "d20, a15, B3";
            
            // When: 파싱과 정규화 동시 실행
            TokenParseResult result = TokenParser.ParseAndNormalize(input);
            
            // Then: 정규화되어 반환
            Assert.AreEqual(3, result.Tokens.Length);
            Assert.AreEqual(3, result.ValidTokenCount);
            Assert.AreEqual("D20", result.Tokens[0]);
            Assert.AreEqual("A15", result.Tokens[1]);
            Assert.AreEqual("B03", result.Tokens[2]);
            
            // 모든 토큰이 정상이므로 에러 없음
            Assert.IsNull(result.ErrorMessages[0]);
            Assert.IsNull(result.ErrorMessages[1]);
            Assert.IsNull(result.ErrorMessages[2]);
        }

        [Test]
        public void ParseAndNormalize_잘못된코드포함_부분실패()
        {
            // Given: 정상 코드와 잘못된 코드 혼합
            string input = "D20, INVALID, A15";
            
            // When: 파싱과 정규화 동시 실행
            TokenParseResult result = TokenParser.ParseAndNormalize(input);
            
            // Then: 전체 3개 중 2개만 유효
            Assert.AreEqual(3, result.Tokens.Length);
            Assert.AreEqual(2, result.ValidTokenCount);
            
            // 정상 처리된 토큰들
            Assert.AreEqual("D20", result.Tokens[0]);
            Assert.AreEqual("A15", result.Tokens[2]);
            Assert.IsNull(result.ErrorMessages[0]);
            Assert.IsNull(result.ErrorMessages[2]);
            
            // 실패한 토큰
            Assert.AreEqual("INVALID", result.Tokens[1]);
            Assert.IsNotNull(result.ErrorMessages[1]);
            Assert.That(result.ErrorMessages[1], Contains.Substring("잘못된 코드 형식"));
        }

        #endregion

        #region 유틸리티 메서드 테스트

        [Test]
        public void CountTokens_정상입력_개수반환()
        {
            // Given: 다양한 구분자 입력
            string input = "D20, A15 B03,C07";
            
            // When: 토큰 개수 확인
            int count = TokenParser.CountTokens(input);
            
            // Then: 4개 반환
            Assert.AreEqual(4, count);
        }

        [Test]
        public void HasValidTokens_정상코드_true반환()
        {
            // Given: 정상적인 코드들
            string input = "D20, A15";
            
            // When: 유효 토큰 존재 여부 확인
            bool hasValid = TokenParser.HasValidTokens(input);
            
            // Then: true 반환
            Assert.IsTrue(hasValid);
        }

        [Test]
        public void HasValidTokens_모두잘못된코드_false반환()
        {
            // Given: 모두 잘못된 형식
            string input = "INVALID1, INVALID2";
            
            // When: 유효 토큰 존재 여부 확인
            bool hasValid = TokenParser.HasValidTokens(input);
            
            // Then: false 반환
            Assert.IsFalse(hasValid);
        }

        [Test]
        public void HasValidTokens_혼합코드_true반환()
        {
            // Given: 정상 코드와 잘못된 코드 혼합
            string input = "D20, INVALID";
            
            // When: 유효 토큰 존재 여부 확인
            bool hasValid = TokenParser.HasValidTokens(input);
            
            // Then: 하나라도 유효하면 true
            Assert.IsTrue(hasValid);
        }

        #endregion

        #region AC 연계 테스트 (수용기준 검증)

        [Test]
        public void AC1_연계테스트_D20A15입력시_2개코드파싱()
        {
            // Given: AC-1 시나리오 - "D20, A15" 입력
            string input = "D20, A15";
            
            // When: 파싱 실행
            string[] result = TokenParser.ParseTokens(input);
            
            // Then: 2개 코드가 올바르게 파싱됨
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("D20", result[0]);
            Assert.AreEqual("A15", result[1]);
        }

        #endregion
    }
}