using System.Collections;
using Data;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests_PlayMode
{
    // T-304: 요약 집계 SimulationManager 통합 테스트
    // AC-9.1: 남은 타깃이 0개가 되면 시뮬레이션을 중단하고 요약 표시
    // AC-9.2: ROUTE_TIMEOUT 등 모든 실패 사유가 요약에 반영됨
    public class SummaryIntegrationTest
    {
        private GameObject simManagerObj;
        private SimulationManager simManager;
        private Core.SimulationConfig config;

        [SetUp]
        public void SetUp()
        {
            // 설정 생성
            config = ScriptableObject.CreateInstance<Core.SimulationConfig>();
            config.handleTime = 2f;
            config.robotSpeed = 3f;
            config.moveTimeoutSec = 30f;
            config.topN = 3;
            config.randomSeed = 42;
            config.warehousePos = Vector2Int.zero;

            // SimulationManager 생성
            simManagerObj = new GameObject("SimulationManager");
            simManager = simManagerObj.AddComponent<SimulationManager>();

            // 리플렉션으로 private 필드 설정
            var simConfigField = typeof(SimulationManager).GetField("config",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var summaryField = typeof(SimulationManager).GetField("summary",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            simConfigField?.SetValue(simManager, config);
            summaryField?.SetValue(simManager, new Summary()); // summary 직접 초기화
        }

        [TearDown]
        public void TearDown()
        {
            if (simManagerObj != null) Object.Destroy(simManagerObj);
            if (config != null) Object.Destroy(config);
        }

        [UnityTest]
        public IEnumerator SetTotalTargets_SetsCorrectValue()
        {
            // When: 총 타깃 수 설정
            simManager.SetTotalTargets(10);

            // Then: Summary에 반영됨
            var summary = simManager.GetSummary();
            Assert.IsNotNull(summary, "Summary가 null이 아니어야 함");
            Assert.AreEqual(10, summary.totalTargets, "totalTargets가 10이어야 함");

            yield return null;
        }

        [UnityTest]
        public IEnumerator RecordSuccess_IncrementsSummary()
        {
            // Given: 총 타깃 설정
            simManager.SetTotalTargets(5);

            // When: 성공 기록
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();

            // Then: Summary에 반영됨
            var summary = simManager.GetSummary();
            Assert.AreEqual(3, summary.attempted);
            Assert.AreEqual(3, summary.success);
            Assert.AreEqual(0, summary.failed);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RecordFailure_IncrementsSummaryAndReasons()
        {
            // Given: 총 타깃 설정
            simManager.SetTotalTargets(5);

            // When: 실패 기록
            simManager.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            simManager.RecordFailure(ErrorCode.CAPACITY_FULL);

            // Then: Summary에 반영됨
            var summary = simManager.GetSummary();
            Assert.AreEqual(2, summary.attempted);
            Assert.AreEqual(0, summary.success);
            Assert.AreEqual(2, summary.failed);
            Assert.AreEqual(2, summary.reasons.Count);
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.CAPACITY_FULL]);

            yield return null;
        }

        [UnityTest]
        public IEnumerator RecordFailure_RouteTimeout_RecordedCorrectly_AC92()
        {
            // AC-9.2: ROUTE_TIMEOUT이 요약에 반영되는지 검증
            // Given: 총 타깃 설정
            simManager.SetTotalTargets(3);

            // When: ROUTE_TIMEOUT 실패 기록
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);

            // Then: Summary에 ROUTE_TIMEOUT 반영됨
            var summary = simManager.GetSummary();
            Assert.AreEqual(2, summary.failed);
            Assert.IsTrue(summary.reasons.ContainsKey(ErrorCode.ROUTE_TIMEOUT));
            Assert.AreEqual(2, summary.reasons[ErrorCode.ROUTE_TIMEOUT]);

            yield return null;
        }

        [UnityTest]
        public IEnumerator MixedSuccessAndFailure_RecordsCorrectly()
        {
            // Given: AC-9.1 시나리오
            simManager.SetTotalTargets(8);

            // When: 성공 6개, 실패 2개 기록
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            simManager.RecordFailure(ErrorCode.CAPACITY_FULL);

            // Then: Summary가 정확히 집계됨
            var summary = simManager.GetSummary();
            Assert.AreEqual(8, summary.totalTargets);
            Assert.AreEqual(8, summary.attempted);
            Assert.AreEqual(6, summary.success);
            Assert.AreEqual(2, summary.failed);
            Assert.AreEqual(2, summary.reasons.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSummary_ReturnsCorrectSummary()
        {
            // Given: 데이터 기록
            simManager.SetTotalTargets(10);
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);

            // When: GetSummary 호출
            var summary = simManager.GetSummary();

            // Then: 올바른 Summary 반환
            Assert.IsNotNull(summary);
            Assert.AreEqual(10, summary.totalTargets);
            Assert.AreEqual(3, summary.attempted);
            Assert.AreEqual(2, summary.success);
            Assert.AreEqual(1, summary.failed);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Summary_ToString_OutputsStandardFormat_AC91()
        {
            // AC-9.1: 표준 포맷 출력 검증
            // Given: AC-9.1 예제 데이터
            simManager.SetTotalTargets(8);
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            simManager.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            simManager.RecordFailure(ErrorCode.CAPACITY_FULL);

            // When: ToString 호출
            var summary = simManager.GetSummary();
            string result = summary.ToString();

            // Then: 표준 포맷 확인
            StringAssert.Contains("summary:", result);
            StringAssert.Contains("- total_targets: 8", result);
            StringAssert.Contains("- attempted: 8", result);
            StringAssert.Contains("- success: 6", result);
            StringAssert.Contains("- failed: 2", result);
            StringAssert.Contains("ROUTE_BLOCKED:1", result);
            StringAssert.Contains("CAPACITY_FULL:1", result);

            // Debug.Log로 출력 확인
            Debug.Log("AC-9.1 표준 포맷 출력:\n" + result);

            yield return null;
        }

        [UnityTest]
        public IEnumerator MultipleFailuresOfSameType_CountsCorrectly()
        {
            // Given: 같은 타입의 실패 여러 개
            simManager.SetTotalTargets(10);
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            simManager.RecordFailure(ErrorCode.ROUTE_TIMEOUT);
            simManager.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            simManager.RecordSuccess();

            // When: Summary 확인
            var summary = simManager.GetSummary();

            // Then: 같은 타입이 누적됨
            Assert.AreEqual(5, summary.attempted);
            Assert.AreEqual(1, summary.success);
            Assert.AreEqual(4, summary.failed);
            Assert.AreEqual(3, summary.reasons[ErrorCode.ROUTE_TIMEOUT]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED]);

            yield return null;
        }
    }
}
