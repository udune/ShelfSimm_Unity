using System.Collections;
using Core;
using Data;
using Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests_PlayMode
{
    // T-307: Pause/Resume 타이머 동작 통합 테스트
    // AC-20.3: Pause 시 로봇 이동·타이머가 정지되고, Resume 시 동일 상태에서 이어짐
    // AC-7: 로봇이 접근 타일 도달 후 2초 대기하고 재고 수량 정확히 반영
    // AC-7.1: handle_time을 3초로 변경하면 처리 지연이 정확히 반영됨
    public class PauseResumeTimerIntegrationTest
    {
        private GameObject simManagerObj;
        private GameObject robotObj;
        private SimulationManager simManager;
        private RobotController robotController;
        private SimulationConfig config;

        [SetUp]
        public void SetUp()
        {
            // 설정 생성
            config = ScriptableObject.CreateInstance<SimulationConfig>();
            config.handleTime = 2f;
            config.robotSpeed = 3f;
            config.moveTimeoutSec = 30f;
            config.topN = 3;
            config.randomSeed = 42;
            config.warehousePos = Vector2Int.zero;

            // SimulationManager 생성
            simManagerObj = new GameObject("SimulationManager");
            simManager = simManagerObj.AddComponent<SimulationManager>();
            
            // RobotController 생성
            robotObj = new GameObject("Robot");
            robotController = robotObj.AddComponent<RobotController>();
            
            // 리플렉션으로 private 필드 설정
            var simConfigField = typeof(SimulationManager).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var robotConfigField = typeof(RobotController).GetField("config", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var robotControllerField = typeof(SimulationManager).GetField("robotController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            simConfigField?.SetValue(simManager, config);
            robotConfigField?.SetValue(robotController, config);
            robotControllerField?.SetValue(simManager, robotController);
        }

        [TearDown]
        public void TearDown()
        {
            if (robotObj != null) Object.Destroy(robotObj);
            if (simManagerObj != null) Object.Destroy(simManagerObj);
            if (config != null) Object.Destroy(config);
        }

        [UnityTest]
        public IEnumerator Pause_StopsHandlingTimer()
        {
            // Given: 로봇이 HANDLING 상태로 작업 시작
            robotController.TransitionTo(RobotState.HANDLING);
            yield return new WaitForSeconds(0.5f); // 0.5초 경과
            
            // When: 일시정지 실행
            simManager.TogglePause();
            yield return new WaitForSeconds(1f); // 1초 대기
            
            // Then: 여전히 HANDLING 상태 유지 (타이머 정지로 완료 안됨)
            Assert.AreEqual(RobotState.HANDLING, robotController.CurrentState, 
                "Pause 상태에서는 작업이 완료되지 않아야 함");
            Assert.IsTrue(robotController.IsPaused, "일시정지 상태가 true여야 함");
        }

        [UnityTest]
        public IEnumerator Resume_ContinuesTimerFromPausedPoint()
        {
            // Given: 로봇이 HANDLING 상태로 작업 시작 후 1초 경과
            robotController.TransitionTo(RobotState.HANDLING);
            yield return new WaitForSeconds(1f);
            
            // When: 일시정지 후 1초 대기, 다시 재개
            simManager.TogglePause();
            yield return new WaitForSeconds(1f); // 이 시간은 카운트 안됨
            simManager.TogglePause(); // Resume
            
            // Then: 남은 1초 후 작업 완료 (총 실제 작업시간 2초)
            yield return new WaitForSeconds(1.2f);
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState, 
                "Resume 후 남은 시간만큼만 대기하고 완료되어야 함");
            Assert.IsFalse(robotController.IsPaused, "재개 후 일시정지 상태가 false여야 함");
        }

        [UnityTest]
        public IEnumerator HandleTime_ChangeReflectedAccurately()
        {
            // Given: handle_time을 3초로 변경
            simManager.UpdateHandleTime(3f);
            
            // When: 작업 시작
            robotController.TransitionTo(RobotState.HANDLING);
            
            // Then: 2초 후에는 아직 완료 안됨
            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(RobotState.HANDLING, robotController.CurrentState, 
                "3초 설정 시 2.5초 후에는 아직 완료 안됨");
            
            // Then: 3초 후에는 완료됨
            yield return new WaitForSeconds(0.7f);
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState, 
                "3초 설정 시 정확히 3초 후 완료되어야 함");
        }

        [UnityTest]
        public IEnumerator MultiplePauseResumeCycles_WorkCorrectly()
        {
            // Given: 2초 작업 시작
            robotController.TransitionTo(RobotState.HANDLING);
            
            // When: 0.5초마다 Pause/Resume 반복
            yield return new WaitForSeconds(0.5f);
            simManager.TogglePause(); // Pause
            yield return new WaitForSeconds(0.3f);
            
            simManager.TogglePause(); // Resume
            yield return new WaitForSeconds(0.5f);
            
            simManager.TogglePause(); // Pause
            yield return new WaitForSeconds(0.3f);
            
            simManager.TogglePause(); // Resume
            yield return new WaitForSeconds(1.2f); // 남은 시간 대기
            
            // Then: 최종적으로 IDLE 상태로 완료
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState, 
                "여러 번 Pause/Resume해도 실제 작업 시간만큼만 경과하면 완료되어야 함");
        }

        [UnityTest]
        public IEnumerator Pause_DoesNotAffectOtherStates()
        {
            // Given: IDLE 상태에서 일시정지
            robotController.TransitionTo(RobotState.IDLE);
            simManager.TogglePause();
            
            // Then: IDLE 상태 유지
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(RobotState.IDLE, robotController.CurrentState);
            
            // When: 다른 상태로 전환 시도
            robotController.TransitionTo(RobotState.MOVING);
            
            // Then: 상태 전환은 가능 (Pause는 Update만 막음)
            Assert.AreEqual(RobotState.MOVING, robotController.CurrentState, 
                "Pause 중에도 상태 전환은 가능해야 함");
        }

        [Test]
        public void GetHandleTime_ReturnsCorrectValue()
        {
            // Given & When
            float result = simManager.GetHandleTime();
            
            // Then
            Assert.AreEqual(2f, result, 0.001f, "초기 handle_time은 2초여야 함");
        }

        [Test]
        public void UpdateHandleTime_WithZeroOrNegative_DoesNotUpdate()
        {
            // Given
            float initialValue = simManager.GetHandleTime();
            
            // When: 0 또는 음수 입력
            simManager.UpdateHandleTime(0f);
            float afterZero = simManager.GetHandleTime();
            
            simManager.UpdateHandleTime(-1f);
            float afterNegative = simManager.GetHandleTime();
            
            // Then: 값 변경 안됨
            Assert.AreEqual(initialValue, afterZero, "0 입력 시 값 변경 안됨");
            Assert.AreEqual(initialValue, afterNegative, "음수 입력 시 값 변경 안됨");
        }
    }
}