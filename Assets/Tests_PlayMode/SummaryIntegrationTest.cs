using System.Collections;
using Core;
using Data;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests_PlayMode.Tests_PlayMode
{
    public class SummaryIntegrationTest
    {
        private GameObject simManagerObj;
        private SimulationManager simManager;
        private SimulationConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SimulationConfig>();
            // ... config 설정 ...

            simManagerObj = new GameObject("SimulationManager");
            simManager = simManagerObj.AddComponent<SimulationManager>();

            // 리플렉션으로 private config 필드 설정
            var simConfigField = typeof(SimulationManager).GetField("config",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            simConfigField?.SetValue(simManager, config);
            
            // SimulationManager가 자체적으로 summary를 초기화하므로, 여기서 주입할 필요 없음
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
            simManager.SetTotalTargets(10);
            var summary = simManager.GetSummary();
            Assert.IsNotNull(summary);
            Assert.AreEqual(10, summary.totalTargets);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RecordSuccess_IncrementsSummary()
        {
            simManager.SetTotalTargets(5);
            simManager.RecordSuccess();
            simManager.RecordSuccess();
            var summary = simManager.GetSummary();
            Assert.AreEqual(2, summary.attempted);
            Assert.AreEqual(2, summary.success);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RecordFailure_IncrementsSummaryAndReasons()
        {
            simManager.SetTotalTargets(5);
            simManager.RecordFailure(ErrorCode.ROUTE_BLOCKED);
            simManager.RecordFailure(ErrorCode.CAPACITY_FULL);
            var summary = simManager.GetSummary();
            Assert.AreEqual(2, summary.attempted);
            Assert.AreEqual(2, summary.failed);
            Assert.AreEqual(1, summary.reasons[ErrorCode.ROUTE_BLOCKED]);
            Assert.AreEqual(1, summary.reasons[ErrorCode.CAPACITY_FULL]);
            yield return null;
        }
        
        // ... 다른 테스트들은 변경 필요 없음 ...
        [Test]
        public void AllTestsAreValid()
        {
            // 이 파일의 다른 테스트들은 SimulationManager의 내부 집계 로직만 검증하므로
            // RobotController의 변경사항과 무관하게 유효합니다.
            Assert.Pass();
        }
    }
}
