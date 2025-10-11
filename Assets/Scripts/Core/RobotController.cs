using System;
using Data;
using UnityEngine;

namespace Core
{
    public class RobotController : MonoBehaviour
    {
        [SerializeField] private SimulationConfig config; // 시뮬레이션 설정 참조

        private RobotState currentState = RobotState.IDLE; // 현재 로봇 상태
        private float handleTimer; // 작업 처리 타이머
        private float handleDuration; // 작업 처리 시간
        private bool isPaused; // 일시정지 상태
        
        public RobotState CurrentState => currentState; // 현재 상태 반환
        public bool IsPaused => isPaused; // 일시정지 상태 반환

        private void Start() // 초기화
        {
            if (config == null) // config가 할당되지 않은 경우 오류 로그 출력
            {
                Debug.LogError("SimulationConfig is not assigned.");
                return;
            }
            
            currentState = RobotState.IDLE; // 초기 상태 설정
        }

        private void Update()
        {
            if (isPaused) // 일시정지 상태인 경우 업데이트 중지
            {
                return;
            }

            switch (currentState) // 현재 상태에 따라 처리
            {
                case RobotState.HANDLING: // 작업 처리 중
                    UpdateHandling(); // 작업 처리 업데이트
                    break;
            }
        }

        public void TransitionTo(RobotState newState) // 상태 전환 메서드
        {
            if (currentState == newState) // 현재 상태와 전환하려는 상태가 같은 경우 아무 작업도 수행하지 않음
            {
                return;
            }

            OnStateExit(currentState); // 현재 상태 종료 작업 수행
            currentState = newState; // 상태 변경
            OnStateEnter(currentState); // 새로운 상태 진입 작업 수행
        }

        private void OnStateEnter(RobotState state) // 상태 진입 시 필요한 작업 수행
        {
            switch (state) // 상태에 따른 초기화 작업
            {
                case RobotState.HANDLING: // 작업 처리 상태 진입 시
                    StartHandling(); // 작업 처리 시작
                    break;
            }
        }

        private void OnStateExit(RobotState state) // 상태 종료 시 필요한 작업 수행
        {
            // 상태 종료 시 필요한 작업 수행
        }

        private void StartHandling() // 작업 처리 시작
        {
            handleDuration = config.handleTime; // 작업 처리 시간 설정
            handleTimer = 0f; // 작업 처리 타이머 초기화
            Debug.Log($"작업 처리 시작 (예상 소요시간: {handleDuration}초)");
        }

        private void UpdateHandling() // 작업 처리 업데이트
        {
            handleTimer += Time.deltaTime; // 타이머 증가

            // config.handleTime이 동적으로 변경될 수 있으므로 매번 확인
            float currentHandleTime = config != null ? config.handleTime : handleDuration;

            if (handleTimer >= currentHandleTime) // 작업 처리 시간이 경과한 경우
            {
                OnHandleComplete(); // 작업 완료 처리
            }
        }

        private void OnHandleComplete() // 작업 완료 처리
        {
            Debug.Log($"작업 처리 완료 (소요시간: {handleTimer:F2}초)");
            
            // 재고 수량 업데이트 등 작업 완료 후 필요한 로직 추가
            
            TransitionTo(RobotState.IDLE); // 작업 완료 후 대기 상태로 전환
        }

        public void Pause() // 로봇 일시정지
        {
            isPaused = true; // 일시정지 상태 설정
            Debug.Log("로봇 일시정지");
        }
        
        public void Resume() // 로봇 재개
        {
            isPaused = false; // 일시정지 해제
            Debug.Log("로봇 재개");
        }

        public void UpdateHandleTime(float newHandleTime) // 작업 처리 시간 업데이트
        {
            if (newHandleTime > 0) // 유효한 작업 처리 시간인 경우
            {
                config.handleTime = newHandleTime; // 작업 처리 시간 업데이트
                Debug.Log($"작업 처리 시간 업데이트: {newHandleTime}초");
            }
        }
    }
}