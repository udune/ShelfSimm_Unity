using System.Collections;
using Core;
using Data;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests_PlayMode
{
    public class PauseResumeTimerIntegrationTest
    {
        private GameObject simManagerObj;
        private GameObject robotObj;
        private SimulationManager simManager;
        private RobotController robotController;
        private SimulationConfig config;

        // 테스트용 데이터
        private Job testJob;
        private Cell testCell;
        private Book testBook;

        [SetUp]
        public void SetUp()
        {
            // 설정 생성
            config = ScriptableObject.CreateInstance<SimulationConfig>();
            config.handleTime = 2f;

            // SimulationManager, RobotController 생성 및 연결
            simManagerObj = new GameObject("SimulationManager");
            simManager = simManagerObj.AddComponent<SimulationManager>();
            robotObj = new GameObject("Robot");
            robotController = robotObj.AddComponent<RobotController>();

            var simConfigField = typeof(SimulationManager).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var robotConfigField = typeof(RobotController).GetField("config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var robotControllerField = typeof(SimulationManager).GetField("robotController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            simConfigField?.SetValue(simManager, config);
            robotConfigField?.SetValue(robotController, config);
            robotControllerField?.SetValue(simManager, robotController);

            // 테스트용 데이터 생성
            testBook = new Book("Test Book", 30, 100);
            testCell = new Cell("A01", 100, 120);
            testJob = new Job(Data.JobAction.PUT, "A01", "Test Book", 1);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(robotObj);
            Object.Destroy(simManagerObj);
            Object.Destroy(config);
        }

        private void StartTestJob()
        {
            // 네 번째 인자로 빈 콜백 함수를 전달
            robotController.StartJob(testJob, testCell, testBook, _ => {});
        }

        [UnityTest]
        public IEnumerator Pause_StopsHandlingTimer()
        {
            // Given: 로봇이 작업을 시작
            StartTestJob();
            yield return new WaitForSeconds(0.5f);
            
            // When: 일시정지
            simManager.TogglePause();
            yield return new WaitForSeconds(1f);
            
            // Then: 여전히 HANDLING 상태 유지
            Assert.AreEqual(Core.RobotState.HANDLING, robotController.CurrentState);
            Assert.IsTrue(robotController.IsPaused);
        }

        [UnityTest]
        public IEnumerator Resume_ContinuesTimerFromPausedPoint()
        {
            // Given: 작업 시작 후 1초 경과
            StartTestJob();
            yield return new WaitForSeconds(1f);
            
            // When: 일시정지 -> 1초 대기 -> 재개
            simManager.TogglePause();
            yield return new WaitForSeconds(1f);
            simManager.TogglePause(); // Resume
            
            // Then: 남은 1초 후 작업 완료
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(Core.RobotState.IDLE, robotController.CurrentState);
            Assert.IsFalse(robotController.IsPaused);
        }

        [UnityTest]
        public IEnumerator HandleTime_ChangeReflectedAccurately()
        {
            // Given: handle_time을 3초로 변경
            simManager.UpdateHandleTime(3f);
            
            // When: 작업 시작
            StartTestJob();
            
            // Then: 2.5초 후에는 아직 완료 안됨
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(Core.RobotState.HANDLING, robotController.CurrentState);
            
            // Then: 3초 후에는 완료됨
            yield return new WaitForSeconds(0.7f);
            Assert.AreEqual(Core.RobotState.IDLE, robotController.CurrentState);
        }
    }
}
