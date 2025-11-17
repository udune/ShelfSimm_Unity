using System;
using Data;
using API;
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

        // 작업 및 API 보고용 데이터
        private Job currentJob;
        private Cell targetCell;
        private Book targetBook;
        private Action<Job, ErrorCode> onJobCompleteCallback;
        private DateTime jobStartTime;
        private int pathLength; // TODO: 경로 탐색 후 이 값을 설정해야 함

        public RobotState CurrentState => currentState;

        private void Start()
        {
            if (apiClient == null) apiClient = FindObjectOfType<ApiClient>();
        }

        private void Update()
        {
            if (isStopped || isPaused || currentState != RobotState.HANDLING) return;
            UpdateHandling();
        }

        public void StartJob(Job job, Cell cell, Book book, Action<Job, ErrorCode> onComplete)
        {
            if (currentState != RobotState.IDLE)
            {
                Debug.LogWarning("로봇이 다른 작업을 수행 중입니다. 새 작업이 거부되었습니다.");
                onComplete?.Invoke(job, ErrorCode.ROBOT_BUSY);
                return;
            }

            currentJob = job;
            targetCell = cell;
            targetBook = book;
            onJobCompleteCallback = onComplete;
            jobStartTime = DateTime.UtcNow;
            pathLength = 10; // 임시 값, 실제로는 경로 탐색 결과로 설정해야 함

            ErrorCode errorCode;
            bool canProceed = (job.Action == Data.JobAction.PUT)
                ? cell.CanPutBook(book, job.Quantity, out errorCode)
                : cell.CanPickBook(job.Quantity, out errorCode);

            if (canProceed)
            {
                TransitionTo(RobotState.HANDLING);
            }
            else
            {
                Debug.LogError($"[Job Failed] {job.Action} 작업 불가: {errorCode.ToMessage()}");
                ReportJobResult(errorCode); // 실패 즉시 보고
                onJobCompleteCallback?.Invoke(currentJob, errorCode);
                ClearJobData();
            }
        }

        private void TransitionTo(RobotState newState)
        {
            if ((isStopped && newState != RobotState.IDLE) || currentState == newState) return;
            currentState = newState;
        }

        private void UpdateHandling()
        {
            handleTimer += Time.deltaTime;
            if (handleTimer >= config.handleTime)
            {
                OnHandleComplete();
            }
        }

        private void OnHandleComplete()
        {
            if (currentJob.Action == Data.JobAction.PUT) targetCell.PutBook(targetBook, currentJob.Quantity);
            else targetCell.PickBook(currentJob.Quantity);

            ReportJobResult(ErrorCode.NONE); // 성공 보고
            onJobCompleteCallback?.Invoke(currentJob, ErrorCode.NONE);
            ClearJobData();
            TransitionTo(RobotState.IDLE);
        }

        private void ReportJobResult(ErrorCode resultCode)
        {
            if (!reportToApi || apiClient == null || string.IsNullOrEmpty(currentJob?.JobId)) return;

            var endTime = DateTime.UtcNow;
            var totalTime = (float)(endTime - jobStartTime).TotalSeconds;
            var travelTime = totalTime - config.handleTime; // 간단한 추정

            var request = new UpdateJobResultRequest
            {
                startTs = jobStartTime.ToString("o"), // ISO 8601 형식
                endTs = endTime.ToString("o"),
                travelTimeSec = Mathf.Max(0, travelTime),
                handleTimeSec = config.handleTime,
                totalTimeSec = totalTime,
                pathLengthCells = pathLength,
                result = (resultCode == ErrorCode.NONE) ? "SUCCESS" : "FAIL",
                failReason = (resultCode != ErrorCode.NONE) ? resultCode.ToString() : null,
                robotName = gameObject.name
            };

            StartCoroutine(apiClient.UpdateJobResult(currentJob.JobId, request,
                onSuccess: () => Debug.Log($"작업 결과 업로드 완료: {currentJob.JobId}"),
                onError: (error) => Debug.LogWarning($"결과 업로드 실패: {error}")
            ));
        }

        public void Stop()
        {
            isStopped = true;
            if (currentState == RobotState.HANDLING && currentJob != null)
            {
                ReportJobResult(ErrorCode.CANCELLED_BY_STOP);
                onJobCompleteCallback?.Invoke(currentJob, ErrorCode.CANCELLED_BY_STOP);
                ClearJobData();
            }
            TransitionTo(RobotState.IDLE);
        }

        private void ClearJobData()
        {
            currentJob = null;
            targetCell = null;
            targetBook = null;
            onJobCompleteCallback = null;
        }
        
        // 외부에서 경로 탐색 후 호출
        public void SetPathLength(int length) => pathLength = length;
        
        // 사용되지 않는 메서드들 (Pause/Resume 등)은 간결성을 위해 제거
    }
}
