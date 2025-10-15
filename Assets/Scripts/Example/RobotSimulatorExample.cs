using System;
using System.Collections.Generic;
using Core;
using Data;
using UnityEngine;

public class RobotSimulatorExample : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float moveTimeOutSec = 30f; // 이동 타임아웃 시간
    [SerializeField] private int gridWidth = 50; // 그리드 너비
    [SerializeField] private int gridHeight = 50; // 그리드 높이
    
    private RobotData robot; // 로봇 데이터
    private Vector2Int warehousePos = new Vector2Int(0, 0); // 창고 위치
    private HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(); // 장애물 위치 집합

    private void Start() // 초기화
    {
        robot = new RobotData("robot_001", "Alpha", warehousePos, moveTimeOutSec); // 로봇 초기화
        
        obstacles.Add(new Vector2Int(5, 5)); // 장애물 추가 예시
        obstacles.Add(new Vector2Int(5, 6));
        obstacles.Add(new Vector2Int(6, 5));
        
        Vector2Int target = new Vector2Int(10, 10); // 목표 위치 설정
        TryMoveToTarget(target); // 목표 위치로 이동 시도
    }

    private void Update()
    {
        if (RobotFSM.CheckTimeout(robot, Time.time)) // 이동 타임아웃 체크
        {
            string message = robot.errorCode.Value.ToMessage(); // 오류 메시지 변환
            Debug.Log($"[오류] {message}");
            ReturnToWarehouse(); // 창고로 복귀 시도
            return;
        }
        
        if (robot.state == RobotState.MOVING || robot.state == RobotState.RETURNING) // MOVING 또는 RETURNING 상태인 경우
        {
            if (RobotFSM.UpdatePosition(robot)) // 위치 업데이트
            {
                Debug.Log($"[{robot.name}] 위치 업데이트: {robot.position}");
            }

            if (RobotFSM.HasReachedTarget(robot)) // 목표 위치에 도달했는지 확인
            {
                OnReachedTarget(); // 목표 위치 도달 시 처리
            }
        }
    }

    private void TryMoveToTarget(Vector2Int target) // 목표 위치로 이동 시도
    {
        List<Vector2Int> path = PathFinder.FindPath(
            robot.position,
            target,
            obstacles,
            gridWidth,
            gridHeight); // 목표 위치로 가는 경로 찾기

        if (path == null) // 경로를 찾을 수 없음
        {
            Debug.LogError($"[{robot.name}] 경로를 찾을 수 없습니다: {target}");
            RobotFSM.HandleError(robot, ErrorCode.ROUTE_BLOCKED); // 오류 코드 설정
            
            string message = robot.errorCode.Value.ToMessage(); // 오류 메시지 변환
            Debug.Log($"[오류] {message}");
            
            ReturnToWarehouse(); // 창고로 복귀 시도
            return;
        }

        if (RobotFSM.TransitionToMoving(robot, path, Time.time)) // 이동 시작
        {
            Debug.Log($"[{robot.name}] 이동 시작: {robot.position} -> {target}");
        }
    }
    
    private void OnReachedTarget() // 목표 위치 도달 시 처리
    {
        if (robot.state == RobotState.MOVING) // 목표 위치에 도달한 경우
        {
            Debug.Log($"[{robot.name}] 목표 위치 도달: {robot.position}");
            RobotFSM.TransitionToHandling(robot); // HANDLING 상태로 전이
            ReturnToWarehouse(); // 창고로 복귀 시도
        }
        else if (robot.state == RobotState.RETURNING) // 창고에 도달한 경우
        {
            Debug.Log($"[{robot.name}] 창고 복귀 완료: {robot.position}");
            RobotFSM.TransitionToIdle(robot); // IDLE 상태로 전이

            if (robot.errorCode.HasValue) // 오류가 있었던 경우
            {
                string message = robot.errorCode.Value.ToMessage(); // 오류 메시지 변환
                Debug.Log($"[오류] {message}");
            }
            else
            {
                Debug.Log($"[{robot.name}] 작업 완료");
            }
        }
    }
    
    private void ReturnToWarehouse() // 창고로 복귀 시도
    {
        List<Vector2Int> returnPath = PathFinder.FindPath(
            robot.position,
            warehousePos,
            obstacles,
            gridWidth,
            gridHeight); // 창고로 가는 경로 찾기

        if (returnPath == null) // 창고로 가는 경로를 찾을 수 없음
        {
            Debug.LogError($"[{robot.name}] 창고로 복귀할 수 없습니다: {robot.position}");
            RobotFSM.TransitionToIdle(robot); // 강제 IDLE 전이
            return;
        }
        
        if (RobotFSM.TransitionToReturning(robot, returnPath, Time.time)) // 창고로 복귀 시작
        {
            Debug.Log($"[{robot.name}] 창고로 복귀 시작: {robot.position} -> {warehousePos}");
        }
    }
}
