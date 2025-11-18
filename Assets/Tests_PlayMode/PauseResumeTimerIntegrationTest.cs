using System.Collections;
using Core.Core;
using Data.Data;
using Managers.Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests_PlayMode.Tests_PlayMode
{
    public class PauseResumeTimerIntegrationTest
    {
        private GameObject simManagerObj;
        private GameObject robotObj;
        private SimulationManager simManager;
        private RobotController robotController;
        private SimulationConfig config;

        private Job testJob;
        private Cell testCell;
        private Book testBook;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<SimulationConfig>();
            config.handleTime = 2f;

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

            testBook = new Book("Test Book", 30, 100);
            testCell = new Cell("A01", 100, 120);
            testJob = new Job(JobAction.PUT, "A01", "Test Book", 1);
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
            robotController.StartJob(testJob, testCell, testBook, (job, errorCode) => {});
        }

        [UnityTest]
        public IEnumerator Pause_StopsHandlingTimer()
        {
            StartTestJob();
            yield return new WaitForSeconds(0.5f);
            
            simManager.TogglePause();
            yield return new WaitForSeconds(1f);
            
            Assert.AreEqual(RobotState.HANDLING, robotController.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Resume_ContinuesTimerFromPausedPoint()
        {
            StartTestJob();
            yield return new WaitForSeconds(1f);
            
            simManager.TogglePause();
            yield return new WaitForSeconds(1f);
            simManager.TogglePause(); // Resume
            
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator HandleTime_ChangeReflectedAccurately()
        {
            // Given: handle_time을 3초로 변경
            config.handleTime = 3f;
            
            // When: 작업 시작
            StartTestJob();
            
            // Then: 2.5초 후에는 아직 완료 안됨
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(RobotState.HANDLING, robotController.CurrentState);
            
            // Then: 3초 후에는 완료됨
            yield return new WaitForSeconds(0.7f);
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState);
        }
    }
}
