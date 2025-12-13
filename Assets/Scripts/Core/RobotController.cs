using System;
using System.Collections.Generic;
using API;
using Data;
using UnityEngine;

namespace Core
{
    public class RobotController : MonoBehaviour
    {
        [Header("API 연동")]
        [SerializeField] private bool reportToApi = true;

        [Header("내부 설정")]
        [SerializeField] private SimulationConfig config;
        [SerializeField] private SimpleAStarPathFinder pathFinder;
        [SerializeField] private CellsLayoutSO cellsLayout;

        [Header("시각화")]
        [SerializeField] private Transform robotTransform;
        [SerializeField] private float visualCellSize = 50f;

        private RobotState currentState = RobotState.IDLE;
        private float handleTimer;
        private float moveTimer;
        private bool isPaused;
        private bool isStopped;

        private Job currentJob;
        private Cell targetCell;
        private Book targetBook;
        private Action<Job, ErrorCode> onJobCompleteCallback;
        private DateTime jobStartTime;
        private int pathLength;

        private List<Vector2Int> currentPath;
        private int pathIndex;
        private Vector2Int currentGridPosition;
        private float cellMoveTimer;

        private void Update()
        {
            if (isStopped || isPaused || config == null)
            {
                return;
            }

            switch (currentState)
            {
                case RobotState.MOVING:
                    UpdateMoving();
                    break;
                case RobotState.HANDLING:
                    UpdateHandling();
                    break;
                case RobotState.RETURNING:
                    UpdateReturning();
                    break;
            }
        }

        private void UpdateMoving()
        {
            moveTimer += Time.deltaTime;
            if (moveTimer >= config.moveTimeoutSec)
            {
                Debug.LogError("로봇 이동 타임아웃");
                HandleJobCompletion(ErrorCode.ROUTE_TIMEOUT);
                return;
            }

            MoveAlongPath();
        }

        private void UpdateHandling()
        {
            handleTimer += Time.deltaTime;
            if (handleTimer >= config.handleTime)
            {
                OnHandleComplete();
            }
        }

        private void UpdateReturning()
        {
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            if (currentPath == null || pathIndex >= currentPath.Count)
            {
                OnPathComplete();
                return;
            }

            cellMoveTimer += Time.deltaTime;
            float cellMoveTime = 1f / config.robotSpeed;

            if (cellMoveTimer >= cellMoveTime)
            {
                cellMoveTimer = 0f;
                pathIndex++;

                if (pathIndex < currentPath.Count)
                {
                    currentGridPosition = currentPath[pathIndex];
                    UpdateRobotVisualPosition();
                }
                else
                {
                    OnPathComplete();
                }
            }
        }

        private void UpdateRobotVisualPosition()
        {
            if (robotTransform != null)
            {
                robotTransform.position = new Vector3(
                    currentGridPosition.x * visualCellSize,
                    currentGridPosition.y * visualCellSize,
                    0
                );
            }
        }

        private void OnPathComplete()
        {
            if (currentState == RobotState.MOVING)
            {
                Debug.Log($"로봇이 목표 셀({currentJob.CellCode})에 도착했습니다.");
                TransitionTo(RobotState.HANDLING);
                handleTimer = 0f;
            }
            else if (currentState == RobotState.RETURNING)
            {
                Debug.Log("로봇이 웨어하우스로 복귀했습니다.");
                HandleJobCompletion(ErrorCode.NONE);
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

            if (!canProceed)
            {
                Debug.LogError($"작업 불가: {errorCode.ToMessage()}");
                HandleJobCompletion(errorCode);
                return;
            }

            if (cellsLayout == null)
            {
                Debug.LogError("CellsLayout이 없습니다.");
                HandleJobCompletion(ErrorCode.INVALID_CODE);
                return;
            }

            CellDef cellDef = cellsLayout.GetCellByCode(job.CellCode);
            if (cellDef == null)
            {
                Debug.LogError($"셀을 찾을 수 없습니다: {job.CellCode}");
                HandleJobCompletion(ErrorCode.INVALID_CODE);
                return;
            }

            Vector2Int warehouse = cellsLayout.warehouse;
            Vector2Int goalPos = new Vector2Int(cellDef.X, cellDef.Y);

            if (pathFinder != null)
            {
                currentPath = pathFinder.FindPath(warehouse, goalPos);
            }

            if (currentPath == null || currentPath.Count == 0)
            {
                Debug.LogError($"경로를 찾을 수 없습니다: {warehouse} -> {goalPos}");
                HandleJobCompletion(ErrorCode.ROUTE_BLOCKED);
                return;
            }

            pathIndex = 0;
            currentGridPosition = warehouse;
            moveTimer = 0f;
            cellMoveTimer = 0f;

            Debug.Log($"작업 시작: {job.CellCode}, 경로 길이: {currentPath.Count}");
            TransitionTo(RobotState.MOVING);
        }

        private void OnHandleComplete()
        {
            if (currentJob.Action == JobAction.PUT)
            {
                targetCell.PutBook(targetBook, currentJob.Quantity);
                Debug.Log($"책 입고 완료: {targetBook.Title} x{currentJob.Quantity}");
            }
            else
            {
                targetCell.PickBook(currentJob.Quantity);
                Debug.Log($"책 출고 완료: x{currentJob.Quantity}");
            }

            StartReturning();
        }

        private void StartReturning()
        {
            if (cellsLayout == null)
            {
                HandleJobCompletion(ErrorCode.NONE);
                return;
            }

            Vector2Int warehouse = cellsLayout.warehouse;

            if (pathFinder != null)
            {
                currentPath = pathFinder.FindPath(currentGridPosition, warehouse);
            }

            if (currentPath == null || currentPath.Count == 0)
            {
                Debug.LogWarning("복귀 경로를 찾을 수 없습니다. 작업 완료 처리합니다.");
                HandleJobCompletion(ErrorCode.NONE);
                return;
            }

            pathIndex = 0;
            cellMoveTimer = 0f;
            Debug.Log("웨어하우스로 복귀 시작");
            TransitionTo(RobotState.RETURNING);
        }

        private void HandleJobCompletion(ErrorCode resultCode)
        {
            float totalTime = (float)(DateTime.UtcNow - jobStartTime).TotalSeconds;
            Debug.Log($"작업 완료: {currentJob?.CellCode}, 결과: {resultCode}, 소요시간: {totalTime:F2}초");

            ReportJobResult(resultCode, totalTime);
            onJobCompleteCallback?.Invoke(currentJob, resultCode);

            ClearJobData();
            TransitionTo(RobotState.IDLE);
        }

        private void ReportJobResult(ErrorCode resultCode, float totalTime)
        {
            if (!reportToApi || config == null || string.IsNullOrEmpty(currentJob?.JobId))
            {
                return;
            }

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

            StartCoroutine(ApiClient.Instance.UpdateJobResult(currentJob.JobId, request));
        }

        private void TransitionTo(RobotState newState)
        {
            if ((isStopped && newState != RobotState.IDLE) || currentState == newState) return;

            Debug.Log($"로봇 상태 전환: {currentState} -> {newState}");
            currentState = newState;
        }

        private void ClearJobData()
        {
            currentJob = null;
            targetCell = null;
            targetBook = null;
            onJobCompleteCallback = null;
            currentPath = null;
            pathIndex = 0;
        }

        public void Pause()
        {
            isPaused = true;
            Debug.Log("로봇 일시정지");
        }

        public void Resume()
        {
            isPaused = false;
            Debug.Log("로봇 재개");
        }

        public void Stop()
        {
            isStopped = true;
            if (currentState != RobotState.IDLE && currentJob != null)
            {
                Debug.Log("로봇 정지");
                HandleJobCompletion(ErrorCode.CANCELLED_BY_STOP);
            }
            TransitionTo(RobotState.IDLE);
        }
    }
}
