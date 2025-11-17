using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using API;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private SimulationConfig config;

        [Header("API 연동")]
        [SerializeField] private ApiClient apiClient;
        [SerializeField] private bool useApiMode = true;
        
        [Header("내부 참조")]
        [SerializeField] private RobotController robotController;
        
        public float ElapsedTime { get; private set; }
        public float AverageTaskTime => summary.success > 0 ? ElapsedTime / summary.success : 0;
        
        private Queue<Job> jobQueue;
        private Summary summary;
        private bool isRunning;
        private string currentRunId;
        private bool isPaused;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (config != null) config.OnHandleTimeChanged += HandleTimeChanged;
        }

        private void OnDestroy()
        {
            if (config != null) config.OnHandleTimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float newHandleTime)
        {
            Debug.Log($"[SimulationManager] HandleTime이 {newHandleTime}으로 변경됨을 감지했습니다.");
        }

        private void Start()
        {
            // ... (Start 로직은 이전과 동일)
        }
        
        // ... (다른 메서드들)

        public Summary GetSummary()
        {
            return summary;
        }
        
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }
        
        // --- 이하 코드는 가독성을 위해 생략 (이전과 동일) ---
        private IEnumerator InitializeWithAPI() { yield return null; }
        private void Update() { if (isRunning && !isPaused) ElapsedTime += Time.deltaTime; UpdateDashboard(); }
        private void InitializeSimulation() { summary = new Summary(); jobQueue = new Queue<Job>(); isRunning = true; }
        public void StartSimulationWithJobs(List<Job> jobs) { /* ... */ }
        private void TryProcessNextJob() { /* ... */ }
        private void OnJobFinished(Job job, ErrorCode resultCode) { /* ... */ }
        private List<Job> GetTestJobs() { return new List<Job>(); }
        public void SetTotalTargets(int count) { if(summary != null) summary.totalTargets = count; }
        public void RecordSuccess() { if(summary != null) summary.RecordSuccess(); }
        public void RecordFailure(ErrorCode code) { if(summary != null) summary.RecordFailure(code); }
        private void CheckSimulationComplete() { if(summary != null && summary.attempted >= summary.totalTargets) StopSimulation(); }
        public void StopSimulation() { if (!isRunning) return; isRunning = false; /* ... */ }
        private void UpdateDashboard() { if (UIManager.Instance != null) UIManager.Instance.UpdateDashboard(summary); }
    }
}
