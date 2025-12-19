using System;
using System.Collections.Generic;
using API;
using Data;
using Managers;
using UnityEngine;

namespace Core
{
    public class RobotController : MonoBehaviour
    {
        [Header("내부 설정")]
        [SerializeField] private SimpleAStarPathFinder pathFinder;
        [SerializeField] private GridRenderer gridRenderer;

        [Header("시각화")]
        [SerializeField] private RectTransform robotRectTransform;

        private RobotState currentState = RobotState.IDLE;
        private float handleTimer;
        private float moveTimer;
        private bool isPaused;
        private bool isStopped;

        private Job currentJob;
        private Cell targetCell;
        private BookData targetBook;
        private Action<Job, ErrorCode, JobResult> onJobCompleteCallback;
        private DateTime jobStartTime;
        private int pathLength;

        private List<Vector2Int> currentPath;
        private int pathIndex;
        private Vector2Int currentGridPosition;
        private float cellMoveTimer;

        private void Start()
        {
            currentGridPosition = ConfigManager.Instance.CellsLayout.warehouse;
            UpdateRobotVisualPosition();
        }

        private void Update()
        {
            if (isStopped || isPaused)
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
            if (moveTimer >= ConfigManager.Instance.SimulationConfig.moveTimeoutSec)
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
            if (handleTimer >= ConfigManager.Instance.SimulationConfig.handleTime)
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
            float cellMoveTime = 1f / ConfigManager.Instance.SimulationConfig.robotSpeed;

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
            RectTransform gridRectTransform = gridRenderer.GetComponent<RectTransform>();
            Rect rect = gridRectTransform.rect;

            float cellWidth = gridRenderer.CellWidth;
            float cellHeight = gridRenderer.CellHeight;

            float localX = rect.x + (currentGridPosition.x + 0.5f) * cellWidth;
            float localY = rect.y + (currentGridPosition.y + 0.5f) * cellHeight;

            Vector3 worldPos = gridRectTransform.TransformPoint(new Vector2(localX, localY));

            robotRectTransform.position = worldPos;
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

        public void StartJob(Job job, Cell cell, BookData book, int calculatedPathLength, Action<Job, ErrorCode, JobResult> onComplete)
        {
            if (currentState != RobotState.IDLE)
            {
                onComplete?.Invoke(job, ErrorCode.ROBOT_BUSY, null);
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

            CellDef cellDef = ConfigManager.Instance.CellsLayout.GetCellByCode(job.CellCode);
            if (cellDef == null)
            {
                Debug.LogError($"셀을 찾을 수 없습니다: {job.CellCode}");
                HandleJobCompletion(ErrorCode.INVALID_CODE);
                return;
            }

            Vector2Int warehouse = ConfigManager.Instance.CellsLayout.warehouse;
            Vector2Int targetCellPos = new Vector2Int(cellDef.X, cellDef.Y);

            Vector2Int? accessiblePos = pathFinder?.FindAccessibleNeighbor(targetCellPos, warehouse);
            if (!accessiblePos.HasValue)
            {
                Debug.LogError($"책장에 접근할 수 없습니다: {job.CellCode} (위치: {targetCellPos})");
                HandleJobCompletion(ErrorCode.ROUTE_BLOCKED);
                return;
            }

            if (pathFinder != null)
            {
                currentPath = pathFinder.FindPath(warehouse, accessiblePos.Value);
            }

            if (currentPath == null || currentPath.Count == 0)
            {
                Debug.LogError($"경로를 찾을 수 없습니다: {warehouse} -> {accessiblePos.Value}");
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
            Vector2Int warehouse = ConfigManager.Instance.CellsLayout.warehouse;
            currentPath = pathFinder.FindPath(currentGridPosition, warehouse);

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
            DateTime endTime = DateTime.UtcNow;
            float totalTime = (float)(endTime - jobStartTime).TotalSeconds;
            Debug.Log($"작업 완료: {currentJob?.CellCode}, 결과: {resultCode}, 소요시간: {totalTime:F2}초");

            JobResult jobResult = CreateJobResult(resultCode, endTime, totalTime);
            onJobCompleteCallback?.Invoke(currentJob, resultCode, jobResult);

            ClearJobData();
            TransitionTo(RobotState.IDLE);
        }

        private JobResult CreateJobResult(ErrorCode resultCode, DateTime endTime, float totalTime)
        {
            if (currentJob == null)
            {
                return null;
            }

            float travelTimeSec = (ConfigManager.Instance.SimulationConfig.robotSpeed > 0) ? (pathLength / ConfigManager.Instance.SimulationConfig.robotSpeed) : 0f;
            float calculatedTotalTime = travelTimeSec + ConfigManager.Instance.SimulationConfig.handleTime;

            string resultString = (resultCode == ErrorCode.NONE) ? "Success" : "Failed";
            string failReason = (resultCode == ErrorCode.NONE) ? "" : resultCode.ToString();

            return new JobResult(
                currentJob.JobId,
                jobStartTime,
                endTime,
                travelTimeSec,
                ConfigManager.Instance.SimulationConfig.handleTime,
                calculatedTotalTime,
                pathLength,
                resultString,
                failReason,
                gameObject.name
            );
        }

        private void TransitionTo(RobotState newState)
        {
            if ((isStopped && newState != RobotState.IDLE) || currentState == newState)
            {
                return;
            }

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
