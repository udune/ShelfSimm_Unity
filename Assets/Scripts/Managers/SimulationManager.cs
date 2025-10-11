using System;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    // SimulationManager는 시뮬레이션의 전반적인 관리를 담당하는 싱글톤 클래스입니다.
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; } // 싱글톤 인스턴스
        
        [SerializeField] private SimulationConfig config; // 시뮬레이션 설정
        [SerializeField] private RobotController robotController; // 로봇 컨트롤러 참조

        private bool isPaused; // 시뮬레이션 일시정지 상태

        private void Awake() // 싱글톤 패턴 구현
        {
            if (Instance == null) // Instance가 아직 할당되지 않은 경우
            {
                Instance = this; // 현재 인스턴스를 할당
            }
            else
            {
                Destroy(gameObject); // 이미 인스턴스가 존재하면 현재 게임 오브젝트를 파괴
            }
        }

        private void Start()
        {
            if (config == null) // config가 할당되지 않은 경우 오류 로그 출력
            {
                Debug.LogError("Simulation Manager is null!");
                return;
            }
            
            InitializeSimulation(); // 시뮬레이션 초기화
        }

        private void InitializeSimulation() // 시뮬레이션 초기화 메서드
        {
            Time.fixedDeltaTime = 0.02f; // 고정 업데이트 간격 설정
            
            Random.InitState(config.randomSeed); // 랜덤 시드 초기화
            
            Debug.Log($"시뮬레이션 초기화 완료: Robot Speed = {config.robotSpeed}, Handle Time = {config.handleTime}, Move Timeout = {config.moveTimeoutSec}, Top N = {config.topN}, Warehouse Pos = {config.warehousePos}");
        }

        public void TogglePause() // 시뮬레이션 일시정지 토글 메서드
        {
            isPaused = !isPaused; // 일시정지 상태 토글

            if (robotController != null) // 로봇 컨트롤러가 할당된 경우
            {
                if (isPaused) // 일시정지 상태인 경우
                {
                    robotController.Pause(); // 로봇 컨트롤러 일시정지
                }
                else
                {
                    robotController.Resume(); // 로봇 컨트롤러 재개
                }
            }
        }

        public void UpdateHandleTime(float newValue) // 작업 처리 시간 업데이트 메서드
        {
            if (newValue > 0) // 유효한 작업 처리 시간인 경우
            {
                config.handleTime = newValue; // 작업 처리 시간 업데이트
                
                if (robotController != null) // 로봇 컨트롤러가 할당된 경우
                {
                    robotController.UpdateHandleTime(newValue); // 로봇 컨트롤러에 작업 처리 시간 업데이트
                }
                
                Debug.Log($"작업 처리 시간 업데이트: {newValue}초");
            }
        }

        public float GetHandleTime() // 작업 처리 시간 반환 메서드
        {
            return config.handleTime; // 작업 처리 시간 반환
        }
    }
}