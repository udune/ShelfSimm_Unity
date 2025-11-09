using System;
using System.Collections.Generic;
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
        [SerializeField] private List<Cell> allCells; 
        [SerializeField] private List<Book> allBooks;

        private Queue<Job> jobQueue;
        private Summary summary;
        private bool isRunning;
        
        public float ElapsedTime { get; private set; }
        public float AverageTaskTime => summary.success > 0 ? ElapsedTime / summary.success : 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("SimulationConfig is not assigned!");
                return;
            }
            InitializeSimulation();
            
            StartTestSimulation();
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
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(config.randomSeed);
            summary = new Summary();
            jobQueue = new Queue<Job>();
            isRunning = true;
            
            allCells = new List<Cell> { new Cell("A01", 100, 120), new Cell("B02", 80, 150) };
            allBooks = new List<Book> { new Book("Test Book A", 30, 100), new Book("Test Book B", 25, 130) };
        }

        public void StartSimulationWithJobs(List<Job> jobs)
        {
            if (jobs == null || jobs.Count == 0)
            {
                Debug.LogWarning("시작할 작업이 없습니다.");
                return;
            }

            SetTotalTargets(jobs.Count);
            foreach (var job in jobs)
            {
                jobQueue.Enqueue(job);
            }
            
            TryProcessNextJob();
        }

        private void TryProcessNextJob()
        {
            if (jobQueue.Count > 0)
            {
                Job nextJob = jobQueue.Dequeue();
                
                Cell targetCell = allCells.Find(c => c.CellCode == nextJob.CellCode);
                Book targetBook = allBooks.Find(b => b.Title == nextJob.BookTitle);

                if (targetCell != null && targetBook != null)
                {
                    robotController.StartJob(nextJob, targetCell, targetBook, OnJobFinished);
                }
                else
                {
                    Debug.LogError($"작업 처리 불가: Cell({nextJob.CellCode}) 또는 Book({nextJob.BookTitle})을 찾을 수 없음");
                    RecordFailure(ErrorCode.INVALID_CODE);
                    CheckSimulationComplete();
                }
            }
            else
            {
                Debug.Log("모든 작업이 큐에서 처리되었습니다.");
                CheckSimulationComplete();
            }
        }

        private void OnJobFinished(ErrorCode resultCode)
        {
            if (resultCode == ErrorCode.NONE)
            {
                RecordSuccess();
            }
            else
            {
                RecordFailure(resultCode);
            }
            
            TryProcessNextJob();
        }
        
        private void StartTestSimulation()
        {
            var testJobs = new List<Job>
            {
                new Job(Data.JobAction.PUT, "A01", "Test Book A", 2),
                new Job(Data.JobAction.PUT, "B02", "Test Book B", 3),
                new Job(Data.JobAction.PICK, "A01", "Test Book A", 1)
            };
            StartSimulationWithJobs(testJobs);
        }

        public void SetTotalTargets(int count)
        {
            if (summary != null) summary.totalTargets = count;
        }
        
        public void RecordSuccess()
        {
            if (summary != null) summary.RecordSuccess();
            UpdateDashboard();
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            if (summary != null) summary.RecordFailure(errorCode);
            UpdateDashboard();
        }
        
        private void CheckSimulationComplete()
        {
            if (summary == null) return;
            
            if (summary.totalTargets > 0 && summary.attempted >= summary.totalTargets)
            {
                StopSimulation();
            }
        }

        public void StopSimulation()
        {
            if (!isRunning) return;
            
            isRunning = false;
            Debug.Log("Simulation Stopped.");
            
            if (robotController != null) robotController.Stop();

            while (jobQueue.Count > 0)
            {
                jobQueue.Dequeue();
                RecordFailure(ErrorCode.CANCELLED_BY_STOP);
            }

            Debug.Log(summary.ToString());

            if (UIManager.Instance != null) UIManager.Instance.ShowSummary(summary);
            
            Time.timeScale = 0f;
        }
        
        public Summary GetSummary() => summary;

        private void UpdateDashboard()
        {
            if (UIManager.Instance != null && summary != null)
            {
                UIManager.Instance.UpdateDashboard(summary);
            }
        }

        private bool isPaused;
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (robotController != null)
            {
                if (isPaused) robotController.Pause();
                else robotController.Resume();
            }
        }

        public void UpdateHandleTime(float newValue)
        {
            if (newValue > 0 && config != null)
            {
                config.handleTime = newValue;
                if (robotController != null) robotController.UpdateHandleTime(newValue);
            }
        }

        public float GetHandleTime() => config.handleTime;
    }
}
