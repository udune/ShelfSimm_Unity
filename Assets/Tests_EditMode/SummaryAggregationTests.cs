using Data.Data;
using NUnit.Framework;

namespace Tests_EditMode.Tests_EditMode
{
    // T-304: 요약 집계 (표준 포맷) + UI 표시 테스트
    // AC-9.1: 남은 타깃이 0개가 되면 시뮬레이션을 중단하고 표준 포맷으로 실패 사유 요약 표시
    // AC-9.2: 이동 타임아웃(move_timeout_sec) 초과 시 ROUTE_TIMEOUT 처리 후 복귀/요약에 사유가 반영됨
    public class SummaryAggregationTests
    {
        private Summary summary;

        [SetUp]
        public void SetUp()
        {
            summary = new Summary();
        }

        [Test]
        public void Summary_InitialState_AllFieldsZero()
        {
            // Then: 초기 상태 확인
            Assert.AreEqual(0, summary.totalTargets, "초기 totalTargets는 0이어야 함");
            Assert.AreEqual(0, summary.attempted, "초기 attempted는 0이어야 함");
            Assert.AreEqual(0, summary.success, "초기 success는 0이어야 함");
            Assert.AreEqual(0, summary.failed, "초기 failed는 0이어야 함");
            Assert.AreEqual(0, summary.reasons.Count, "초기 reasons는 비어있어야 함");
        }

        [Test]
        public void RecordSuccess_IncrementsAttemptedAndSuccess()
        {
            // When: 성공 기록
            summary.RecordSuccess();

            // Then: attempted와 success 증가
            Assert.AreEqual(1, summary.attempted, "attempted가 1 증가해야 함");
            Assert.AreEqual(1, summary.success, "success가 1 증가해야 함");
            Assert.AreEqual(0, summary.failed, "failed는 변하지 않아야 함");
        }

        [Test]
        public void RecordSuccess_Multiple_IncrementsCorrectly()
        {
            // When: 성공 3번 기록
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();

            // Then: 올바르게 카운트
            Assert.AreEqual(3, summary.attempted);
            Assert.AreEqual(3, summary.success);
            Assert.AreEqual(0, summary.failed);
        }

        [Test]
        public void RecordFailure_IncrementsAttemptedAndFailed()
        {
            // When: 실패 기록
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);

            // Then: attempted와 failed 증가
            Assert.AreEqual(1, summary.attempted, "attempted가 1 증가해야 함");
            Assert.AreEqual(0, summary.success, "success는 변하지 않아야 함");
            Assert.AreEqual(1, summary.failed, "failed가 1 증가해야 함");
        }

        [Test]
        public void RecordFailure_AddsReasonToDict()
        {
            // When: ROUTE_BLOCKED 실패 기록
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);

            // Then: reasons 딕셔너리에 추가
            Assert.AreEqual(1, summary.reasons.Count, "reasons에 1개 항목이 있어야 함");
            Assert.IsTrue(summary.reasons.ContainsKey(ErrorCode.ROUTE_BLOCKED), "ROUTE_BLOCKED 키가 있어야 함");
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED], "ROUTE_BLOCKED 카운트가 1이어야 함");
        }

        [Test]
        public void RecordFailure_SameErrorCode_IncrementsCount()
        {
            // When: 같은 에러 코드로 3번 실패 기록
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);

            // Then: 카운트가 3으로 증가
            Assert.AreEqual(3, summary.failed);
            Assert.AreEqual(1, summary.reasons.Count, "여전히 1개 키만 있어야 함");
            Assert.AreEqual(3, summary.reasons[ErrorCode.ROUTE_BLOCKED], "ROUTE_BLOCKED 카운트가 3이어야 함");
        }

        [Test]
        public void RecordFailure_DifferentErrorCodes_AddsSeparateEntries()
        {
            // When: 다양한 에러 코드로 실패 기록
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.CAPACITY_FULL);
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);

            // Then: 각각 별도의 항목으로 추가
            Assert.AreEqual(3, summary.failed);
            Assert.AreEqual(3, summary.reasons.Count, "3개의 서로 다른 키가 있어야 함");
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.CAPACITY_FULL]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_TIMEOUT]);
        }

        [Test]
        public void RecordFailure_MixedErrorCodes_CountsCorrectly()
        {
            // When: 혼합된 에러 코드로 기록 (AC-9.1 예제 시나리오)
            summary.totalTargets = 8;
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.CAPACITY_FULL);

            // Then: 요약 정보 검증
            Assert.AreEqual(8, summary.totalTargets);
            Assert.AreEqual(8, summary.attempted);
            Assert.AreEqual(6, summary.success);
            Assert.AreEqual(2, summary.failed);
            Assert.AreEqual(2, summary.reasons.Count);
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.CAPACITY_FULL]);
        }

        [Test]
        public void ToString_EmptyReasons_FormatsCorrectly()
        {
            // Given: 성공만 있는 경우
            summary.totalTargets = 5;
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();

            // When: ToString 호출
            string result = summary.ToString();

            // Then: 표준 포맷 확인
            StringAssert.Contains("summary:", result);
            StringAssert.Contains("- total_targets: 5", result);
            StringAssert.Contains("- attempted: 5", result);
            StringAssert.Contains("- success: 5", result);
            StringAssert.Contains("- failed: 0", result);
            StringAssert.Contains("- reasons: {}", result);
        }

        [Test]
        public void ToString_WithSingleReason_FormatsCorrectly()
        {
            // Given: 단일 실패 사유
            summary.totalTargets = 3;
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);

            // When: ToString 호출
            string result = summary.ToString();

            // Then: 표준 포맷 확인
            StringAssert.Contains("summary:", result);
            StringAssert.Contains("- total_targets: 3", result);
            StringAssert.Contains("- attempted: 3", result);
            StringAssert.Contains("- success: 2", result);
            StringAssert.Contains("- failed: 1", result);
            StringAssert.Contains("ROUTE_TIMEOUT:1", result);
        }

        [Test]
        public void ToString_WithMultipleReasons_FormatsCorrectly_AC91()
        {
            // AC-9.1: 표준 포맷 검증
            // Given: AC-9.1 예제 시나리오
            summary.totalTargets = 8;
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.CAPACITY_FULL);

            // When: ToString 호출
            string result = summary.ToString();

            // Then: AC-9.1 표준 포맷 확인
            StringAssert.Contains("summary:", result);
            StringAssert.Contains("- total_targets: 8", result);
            StringAssert.Contains("- attempted: 8", result);
            StringAssert.Contains("- success: 6", result);
            StringAssert.Contains("- failed: 2", result);

            // reasons 검증 (순서는 상관없음)
            StringAssert.Contains("ROUTE_BLOCKED:1", result);
            StringAssert.Contains("CAPACITY_FULL:1", result);
        }

        [Test]
        public void ToString_RouteTimeout_IncludedInReasons_AC92()
        {
            // AC-9.2: ROUTE_TIMEOUT이 요약에 포함되는지 검증
            // Given: 타임아웃 실패 케이스
            summary.totalTargets = 5;
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);

            // When: ToString 호출
            string result = summary.ToString();

            // Then: ROUTE_TIMEOUT이 요약에 포함됨
            Assert.AreEqual(5, summary.totalTargets);
            Assert.AreEqual(5, summary.attempted);
            Assert.AreEqual(2, summary.success);
            Assert.AreEqual(3, summary.failed);

            StringAssert.Contains("ROUTE_TIMEOUT:2", result);
            StringAssert.Contains("ROUTE_BLOCKED:1", result);
        }

        [Test]
        public void ToString_AllFailureTypes_FormatsCorrectly()
        {
            // Given: 다양한 실패 유형
            summary.totalTargets = 10;
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            summary.RecordFailure(ErrorCode.CAPACITY_FULL);
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            summary.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();
            summary.RecordSuccess();

            // When: ToString 호출
            string result = summary.ToString();

            // Then: 모든 정보가 올바르게 포맷됨
            Assert.AreEqual(10, summary.attempted);
            Assert.AreEqual(4, summary.success);
            Assert.AreEqual(6, summary.failed);

            StringAssert.Contains("- total_targets: 10", result);
            StringAssert.Contains("ROUTE_BLOCKED:3", result);
            StringAssert.Contains("CAPACITY_FULL:1", result);
            StringAssert.Contains("ROUTE_TIMEOUT:2", result);
        }
    }
}
