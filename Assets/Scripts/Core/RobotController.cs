using System;
using API.API;
using Data;
using Data.Data;
using UnityEngine;

namespace Core.Core
{
    public class RobotController : MonoBehaviour
    {
        [Header("API 연동")]
        [SerializeField] private ApiClient apiClient;
        [SerializeField] private bool reportToApi = true;

        [Header("내부 설정")]
        [SerializeField] private SimulationConfig config;

        private RobotState currentState = RobotState.IDLE;
        private float handleTimer;
        private bool isPaused;
        private bool isStopped;

        private Job currentJob;
        private Cell targetCell;
        private Book targetBook;
        private Action<Job, ErrorCode> onJobCompleteCallback;
        private DateTime jobStartTime;
        private int pathLength;

        public RobotState CurrentState => currentState;

        private void Start()
        {
            if (apiClient == null) apiClient = FindObjectOfType<ApiClient>();
        }

        private void Update()
        {
            if (isStopped || isPaused || currentState != RobotState.HANDLING) return;
            
            handleTimer += Time.deltaTime;
            if (handleTimer >= config.handleTime)
            {
                OnHandleComplete();
            }
        }

        public void StartJob(Job job, Cell cell, Book book, Action<Job, ErrorCode> onComplete)
        {
            if (currentState != RobotState.IDLE)
            {
                onComplete?.Invoke(job, ErrorCode.ROBOT_BUSY);
                return;
            }

            currentJob = job;
            targetCell = cell;
            targetBook = book;
            onJobCompleteCallback = onComplete;
            jobStartTime = DateTime.UtcNow;
            pathLength = 10; // 임시 값

            ErrorCode errorCode;
            bool canProceed = (job.Action == JobAction.PUT)
                ? cell.CanPutBook(book, job.Quantity, out errorCode)
                : cell.CanPickBook(job.Quantity, out errorCode);

            if (canProceed)
            {
                TransitionTo(RobotState.HANDLING);
            }
            else
            {
                HandleJobCompletion(errorCode);
            }
        }

        private void OnHandleComplete()
        {
            if (currentJob.Action == JobAction.PUT)
            {
                targetCell.PutBook(targetBook, currentJob.Quantity);
            }
            else
            {
                targetCell.PickBook(currentJob.Quantity);
            }
            HandleJobCompletion(ErrorCode.NONE);
        }

        private void HandleJobCompletion(ErrorCode resultCode)
        {
            float totalTime = (float)(DateTime.UtcNow - jobStartTime).TotalSeconds;
            ReportJobResult(resultCode, totalTime);
            
            onJobCompleteCallback?.Invoke(currentJob, resultCode);
            
            ClearJobData();
            TransitionTo(RobotState.IDLE);
        }

        private void ReportJobResult(ErrorCode resultCode, float totalTime)
        {
            if (!reportToApi || apiClient == null || string.IsNullOrEmpty(currentJob?.JobId)) return;

            var request = new UpdateJobResultRequest
            {
                startTs = jobStartTime.ToString("o"),
                endTs = DateTime.UtcNow.ToString("o"),
                travelTimeSec = Mathf.Max(0, totalTime - config.handleTime),
                handleTimeSec = config.handleTime,
                totalTimeSec = totalTime,
                pathLengthCells = pathLength,
                result = (resultCode == ErrorCode.NONE) ? "SUCCESS" : "FAIL",
                failReason = (resultCode != ErrorCode.NONE) ? resultCode.ToString() : null,
                robotName = gameObject.name
            };

            StartCoroutine(apiClient.UpdateJobResult(currentJob.JobId, request));
        }

        private void TransitionTo(RobotState newState)
        {
            if ((isStopped && newState != RobotState.IDLE) || currentState == newState) return;
            currentState = newState;
        }

        private void ClearJobData()
        {
            currentJob = null;
            targetCell = null;
            targetBook = null;
            onJobCompleteCallback = null;
        }

        public void Pause()
        {
            isPaused = true;
        }
        
        public void Resume()
        {
            isPaused = false;
        }

        public void Stop()
        {
            isStopped = true;
            if (currentState == RobotState.HANDLING && currentJob != null)
            {
                HandleJobCompletion(ErrorCode.CANCELLED_BY_STOP);
            }
            TransitionTo(RobotState.IDLE);
        }
    }
}
