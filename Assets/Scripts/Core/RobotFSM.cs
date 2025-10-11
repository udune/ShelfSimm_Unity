using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    // 로봇 상태 전이 관리
    public static class RobotFSM
    {
        // IDLE 또는 RETURNING 상태에서 MOVING 상태로 전이
        public static bool TransitionToMoving(RobotData robot, List<Vector2Int> path, float currentTime) // path는 시작점 포함, 끝점 포함
        {
            if (robot.state != RobotState.IDLE && robot.state != RobotState.RETURNING) // IDLE 또는 RETURNING 상태에서만 MOVING 상태로 전이 가능
            {
                return false; // 상태 전이 실패
            }

            if (path == null || path.Count == 0) // 경로가 유효하지 않으면 실패
            {
                robot.errorCode = ErrorCode.ROUTE_BLOCKED; // 오류 코드 설정
                return false; // 상태 전이 실패
            }

            robot.state = RobotState.MOVING; // 상태를 MOVING으로 변경
            robot.path = path; // 경로 설정
            robot.pathIndex = 0; // 경로 인덱스 초기화
            robot.targetPosition = path[path.Count - 1]; // 목표 위치 설정 (경로의 마지막 지점)
            robot.moveStartTime = currentTime; // 이동 시작 시간 설정
            robot.errorCode = null; // 오류 코드 초기화

            return true; // 상태 전이 성공
        }

        // MOVING 상태에서 HANDLING 상태로 전이
        public static bool TransitionToHandling(RobotData robot) // 물건을 집거나 내려놓는 상태로 전이
        {
            if (robot.state != RobotState.MOVING) // MOVING 상태에서만 HANDLING 상태로 전이 가능
            {
                return false; // 상태 전이 실패
            }

            robot.state = RobotState.HANDLING; // 상태를 HANDLING으로 변경
            return true; // 상태 전이 성공
        }

        // HANDLING 상태에서 RETURNING 상태로 전이
        public static bool TransitionToReturning(RobotData robot, List<Vector2Int> path, float currentTime) // path는 시작점 포함, 끝점 포함
        {
            if (path == null || path.Count == 0) // 경로가 유효하지 않으면 실패
            {
                TransitionToIdle(robot); // IDLE 상태로 전이
                return false; // 상태 전이 실패
            }
            
            robot.state = RobotState.RETURNING; // 상태를 RETURNING으로 변경
            robot.path = path; // 경로 설정
            robot.pathIndex = 0; // 경로 인덱스 초기화
            robot.targetPosition = path[path.Count - 1]; // 목표 위치 설정 (경로의 마지막 지점)
            robot.moveStartTime = currentTime; // 이동 시작 시간 설정
            return true; // 상태 전이 성공
        }

        // 모든 상태에서 IDLE 상태로 전이
        public static bool TransitionToIdle(RobotData robot) // 모든 상태에서 IDLE 상태로 전이 가능
        {
            robot.state = RobotState.IDLE; // 상태를 IDLE로 변경
            robot.path = null; // 경로 초기화
            robot.pathIndex = 0; // 경로 인덱스 초기화
            robot.targetPosition = null; // 목표 위치 초기화
            robot.moveStartTime = 0f; // 이동 시작 시간 초기화
            
            return true; // 상태 전이 성공
        }

        // MOVING 또는 RETURNING 상태에서 로봇의 현재 위치를 경로에 따라 업데이트
        public static bool UpdatePosition(RobotData robot) // 로봇의 현재 위치를 경로에 따라 업데이트
        {
            if (robot.state != RobotState.MOVING && robot.state != RobotState.RETURNING) // MOVING 또는 RETURNING 상태에서만 위치 업데이트 가능
            {
                return false; // 위치 업데이트 실패
            }
            
            if (robot.path == null || robot.pathIndex >= robot.path.Count) // 경로가 유효하지 않으면 실패
            {
                return false; // 위치 업데이트 실패
            }
            
            robot.position = robot.path[robot.pathIndex]; // 현재 위치를 경로의 현재 인덱스 위치로 업데이트
            robot.pathIndex++; // 다음 경로 인덱스로 이동

            return true; // 위치 업데이트 성공
        }

        // 로봇이 목표 위치에 도달했는지 확인
        public static bool HasReachedTarget(RobotData robot) // 로봇이 목표 위치에 도달했는지 확인
        {
            if (robot.path == null) // 경로가 유효하지 않으면 실패
            {
                return false; // 위치 확인 실패
            }
            
            return robot.pathIndex >= robot.path.Count; // 경로의 끝에 도달했는지 확인
        }

        // MOVING 또는 RETURNING 상태에서 타임아웃 체크
        public static bool CheckTimeout(RobotData robot, float currentTime)
        {
            if (robot.state != RobotState.MOVING && robot.state != RobotState.RETURNING) // MOVING 또는 RETURNING 상태에서만 타임아웃 체크 가능
            {
                return false; // 타임아웃 체크 실패
            }
            
            float elapsed = currentTime - robot.moveStartTime; // 경과 시간 계산
            if (elapsed >= robot.moveTimeoutSec) // 타임아웃 초과 여부 확인
            {
                robot.errorCode = ErrorCode.ROUTE_TIMEOUT; // 오류 코드 설정
                return true; // 타임아웃 발생
            }
            
            return false; // 타임아웃 발생하지 않음
        }
        
        // 오류 발생 시 처리
        public static void HandleError(RobotData robot, ErrorCode errorCode)
        {
            robot.errorCode = errorCode; // 오류 코드 설정
            robot.state = RobotState.IDLE; // 오류 발생 시 IDLE 상태로 전이
        }
    }
}