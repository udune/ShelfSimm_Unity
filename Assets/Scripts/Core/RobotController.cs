using System;
using API;
using Data;
using UnityEngine;

namespace Core
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
            if (isStopped || isPaused || currentState != RobotState.HANDLING || config == null) return;

            handleTimer += Time.deltaTime;
            if (handleTimer >= config.handleTime)
            {
                OnHandleComplete();
            }
        }

        public void StartJob(Job job, Cell cell, Book book, int calculatedPathLength, Action<Job, ErrorCode> onComplete)
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
            pathLength = calculatedPathLength;

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
            if (!reportToApi || apiClient == null || config == null || string.IsNullOrEmpty(currentJob?.JobId)) return;

            // 경로 길이와 로봇 속도로 이동 시간 계산
            float travelTimeSec = (config.robotSpeed > 0) ? (pathLength / config.robotSpeed) : 0f;
            float calculatedTotalTime = travelTimeSec + config.handleTime;

            var request = new UpdateJobResultRequest
            {
                startTs = jobStartTime.ToString("o"),
                endTs = DateTime.UtcNow.ToString("o"),
                travelTimeSec = travelTimeSec,
                handleTimeSec = config.handleTime,
                totalTimeSec = calculatedTotalTime,
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
