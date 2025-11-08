using System;
using Core;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [SerializeField] private SimulationConfig config;
        [SerializeField] private RobotController robotController;

        private bool isPaused;
        private Summary summary;
        
        public float ElapsedTime { get; private set; }
        private bool isRunning;

        public float AverageTaskTime => summary.success > 0 ? ElapsedTime / summary.success : 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("Simulation Manager is null!");
                return;
            }
            
            InitializeSimulation();
        }

        private void Update()
        {
            if (isRunning && !isPaused)
            {
                ElapsedTime += Time.deltaTime;
                UpdateDashboard();
            }
        }

        private void InitializeSimulation()
        {
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(config.randomSeed);

            summary = new Summary();
            isRunning = true;
            
            Debug.Log($"시뮬레이션 초기화 완료: Robot Speed = {config.robotSpeed}, Handle Time = {config.handleTime}, Move Timeout = {config.moveTimeoutSec}, Top N = {config.topN}, Warehouse Pos = {config.warehousePos}");
        }

        public void SetTotalTargets(int count)
        {
            if (summary != null)
            {
                summary.totalTargets = count;
            }
        }
        
        public void RecordSuccess()
        {
            if (summary != null)
            {
                summary.RecordSuccess();
                UpdateDashboard();
            }
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            if (summary != null)
            {
                summary.RecordFailure(errorCode);
                UpdateDashboard();
                CheckSimulationComplete();
            }
        }
        
        private void CheckSimulationComplete()
        {
            if (summary == null) return;
            
            int remaining = summary.totalTargets - summary.attempted;
            if (remaining <= 0)
            {
                StopSimulation();
            }
        }

        public void StopSimulation()
        {
            isRunning = false;
            Time.timeScale = 0f; // Stop time
            Debug.Log("Simulation Stopped.");
            
            if (summary == null) return;

            // 남은 타겟 취소 처리 (실패로 기록)
            int remainingTasks = summary.totalTargets - summary.attempted;
            for (int i = 0; i < remainingTasks; i++)
            {
                summary.RecordFailure(ErrorCode.CANCELLED_BY_STOP);
            }

            Debug.Log(summary.ToString());

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSummary(summary);
            }
            
            if (robotController != null)
            {
                robotController.Stop();
            }
            // TODO: CSV 저장 로직
        }
        
        public Summary GetSummary()
        {
            return summary;
        }

        private void UpdateDashboard()
        {
            if (UIManager.Instance != null && summary != null)
            {
                UIManager.Instance.UpdateDashboard(summary);
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f;
                Debug.Log("Simulation Paused.");
            }
            else
            {
                Time.timeScale = 1f;
                Debug.Log("Simulation Resumed.");
            }

            if (robotController != null)
            {
                if (isPaused)
                {
                    robotController.Pause();
                }
                else
                {
                    robotController.Resume();
                }
            }
        }

        public void UpdateHandleTime(float newValue)
        {
            if (newValue > 0)
            {
                config.handleTime = newValue;
                
                if (robotController != null)
                {
                    robotController.UpdateHandleTime(newValue);
                }
                
                Debug.Log($"작업 처리 시간 업데이트: {newValue}초");
            }
        }

        public float GetHandleTime()
        {
            return config.handleTime;
        }
    }
}
