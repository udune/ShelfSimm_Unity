using System;
using Data;
using UnityEngine;

namespace Core
{
    public class RobotController : MonoBehaviour
    {
        [SerializeField] private SimulationConfig config;

        private RobotState currentState = RobotState.IDLE;
        private float handleTimer;
        private bool isPaused;
        private bool isStopped;

        private Job currentJob;
        private Cell targetCell;
        private Book targetBook;
        private Action<ErrorCode> onJobCompleteCallback;

        public RobotState CurrentState => currentState;
        public bool IsPaused => isPaused;
        public bool IsStopped => isStopped;

        private void Update()
        {
            if (isStopped || isPaused || currentState != Core.RobotState.HANDLING)
            {
                return;
            }
            UpdateHandling();
        }

        public void StartJob(Job job, Cell cell, Book book, Action<ErrorCode> onComplete)
        {
            if (currentState != Core.RobotState.IDLE)
            {
                Debug.LogWarning("로봇이 다른 작업을 수행 중입니다. 새 작업이 거부되었습니다.");
                onComplete?.Invoke(ErrorCode.ROBOT_BUSY);
                return;
            }

            currentJob = job;
            targetCell = cell;
            targetBook = book;
            onJobCompleteCallback = onComplete;

            ErrorCode errorCode;
            bool canProceed = (job.Action == Data.JobAction.PUT)
                ? cell.CanPutBook(book, job.Quantity, out errorCode)
                : cell.CanPickBook(job.Quantity, out errorCode);

            if (canProceed)
            {
                TransitionTo(Core.RobotState.HANDLING);
            }
            else
            {
                Debug.LogError($"[Job Failed] {job.Action} 작업 불가: {errorCode.ToMessage()}");
                onJobCompleteCallback?.Invoke(errorCode);
                ClearJobData();
            }
        }

        private void TransitionTo(Core.RobotState newState)
        {
            if ((isStopped && newState != Core.RobotState.IDLE) || currentState == newState) return;

            currentState = newState;
            if (currentState == Core.RobotState.HANDLING)
            {
                StartHandling();
            }
        }

        private void StartHandling()
        {
            handleTimer = 0f;
            Debug.Log($"[Job Start] {currentJob.Action} 작업 처리 시작 (예상 소요시간: {config.handleTime}초)");
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
            Debug.Log($"[Job Success] 작업 처리 완료 (소요시간: {handleTimer:F2}초)");

            if (currentJob.Action == Data.JobAction.PUT)
            {
                targetCell.PutBook(targetBook, currentJob.Quantity);
            }
            else // PICK
            {
                targetCell.PickBook(currentJob.Quantity);
            }

            onJobCompleteCallback?.Invoke(ErrorCode.NONE);
            ClearJobData();
            TransitionTo(Core.RobotState.IDLE);
        }

        public void Pause()
        {
            if (isStopped) return;
            isPaused = true;
        }
        
        public void Resume()
        {
            if (isStopped) return;
            isPaused = false;
        }

        public void Stop()
        {
            isStopped = true;
            CancelCurrentJob();
            TransitionTo(Core.RobotState.IDLE);
        }
        
        private void CancelCurrentJob()
        {
            if (currentState == Core.RobotState.HANDLING && currentJob != null)
            {
                Debug.Log("현재 작업이 취소되었습니다.");
                onJobCompleteCallback?.Invoke(ErrorCode.CANCELLED_BY_STOP);
                ClearJobData();
            }
        }

        private void ClearJobData()
        {
            currentJob = null;
            targetCell = null;
            targetBook = null;
            onJobCompleteCallback = null;
        }
        
        public void UpdateHandleTime(float newHandleTime)
        {
            if (newHandleTime > 0)
            {
                config.handleTime = newHandleTime;
            }
        }
    }
}
