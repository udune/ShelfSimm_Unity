using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Core;

namespace Tests_EditMode
{
    // T-303: A* 실패/차단/타임아웃 처리 단위 테스트
    public class PathFindingAndTimeoutTests
    {
        [Test]
        public void PathFinder_정상경로_탐색성공()
        {
            // Given: 장애물 없는 환경
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
            Vector2Int start = new Vector2Int(0, 0);
            Vector2Int goal = new Vector2Int(3, 3);
            
            // When: 경로 탐색
            List<Vector2Int> path = PathFinder.FindPath(start, goal, obstacles, 10, 10);
            
            // Then: 경로가 존재하고 시작과 끝이 올바름
            Assert.IsNotNull(path);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(goal, path[path.Count - 1]);
        }
        
        [Test]
        public void PathFinder_장애물차단_경로없음()
        {
            // Given: 목표가 장애물로 막힌 경우
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
            obstacles.Add(new Vector2Int(3, 3));
            
            Vector2Int start = new Vector2Int(0, 0);
            Vector2Int goal = new Vector2Int(3, 3);
            
            // When: 경로 탐색
            List<Vector2Int> path = PathFinder.FindPath(start, goal, obstacles, 10, 10);
            
            // Then: 경로 없음
            Assert.IsNull(path);
        }
        
        [Test]
        public void RobotFSM_경로없음_실패코드설정()
        {
            // Given: 로봇과 null 경로
            RobotData robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0));
            
            // When: 경로 없이 이동 시도
            bool success = RobotFSM.TransitionToMoving(robot, null, 0f);
            
            // Then: 실패하고 ROUTE_BLOCKED 코드 설정
            Assert.IsFalse(success);
            Assert.IsTrue(robot.errorCode.HasValue);
            Assert.AreEqual(ErrorCode.ROUTE_BLOCKED, robot.errorCode.Value);
        }
        
        [Test]
        public void RobotFSM_타임아웃_미발생()
        {
            // Given: 로봇이 이동 중
            RobotData robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0), 10f);
            List<Vector2Int> path = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            RobotFSM.TransitionToMoving(robot, path, 0f);
            
            // When: 5초 후 타임아웃 체크 (제한 10초)
            bool timeout = RobotFSM.CheckTimeout(robot, 5f);
            
            // Then: 타임아웃 미발생
            Assert.IsFalse(timeout);
            Assert.IsFalse(robot.errorCode.HasValue);
        }
        
        [Test]
        public void RobotFSM_타임아웃_발생()
        {
            // Given: 로봇이 이동 중 (타임아웃 5초)
            RobotData robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0), 5f);
            List<Vector2Int> path = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            RobotFSM.TransitionToMoving(robot, path, 0f);
            
            // When: 6초 후 타임아웃 체크
            bool timeout = RobotFSM.CheckTimeout(robot, 6f);
            
            // Then: 타임아웃 발생
            Assert.IsTrue(timeout);
            Assert.IsTrue(robot.errorCode.HasValue);
            Assert.AreEqual(ErrorCode.ROUTE_TIMEOUT, robot.errorCode.Value);
        }
        
        [Test]
        public void RobotFSM_타임아웃_정확한경계값()
        {
            // Given: 로봇이 이동 중 (타임아웃 5초)
            RobotData robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0), 5f);
            List<Vector2Int> path = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            RobotFSM.TransitionToMoving(robot, path, 0f);
            
            // When: 정확히 5초 후
            bool timeout = RobotFSM.CheckTimeout(robot, 5f);
            
            // Then: 타임아웃 발생 (>= 조건)
            Assert.IsTrue(timeout);
            Assert.AreEqual(ErrorCode.ROUTE_TIMEOUT, robot.errorCode.Value);
        }
        
        [Test]
        public void RobotFSM_실패처리_IDLE전환()
        {
            // Given: 로봇이 MOVING 상태
            RobotData robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0));
            List<Vector2Int> path = new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            RobotFSM.TransitionToMoving(robot, path, 0f);
            
            // When: 실패 처리
            RobotFSM.HandleError(robot, ErrorCode.CAPACITY_FULL);
            
            // Then: IDLE 상태로 전환되고 실패 코드 설정
            Assert.AreEqual(RobotState.IDLE, robot.state);
            Assert.IsTrue(robot.errorCode.HasValue);
            Assert.AreEqual(ErrorCode.CAPACITY_FULL, robot.errorCode.Value);
        }
        
        [Test]
        public void ErrorCode_사용자메시지_확인()
        {
            // Given: 각 에러 코드
            ErrorCode[] codes = {
                ErrorCode.ROUTE_BLOCKED,
                ErrorCode.ROUTE_TIMEOUT,
                ErrorCode.CAPACITY_FULL
            };
            
            // When/Then: 각 코드에 대한 메시지가 존재
            foreach (var code in codes)
            {
                string message = code.ToMessage();
                Assert.IsNotNull(message);
                Assert.IsNotEmpty(message);
                Assert.AreNotEqual("알 수 없는 오류입니다", message);
            }
        }
        
        [Test]
        public void PathFinder_맨해튼거리_최단경로()
        {
            // Given: 장애물 없는 환경
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
            Vector2Int start = new Vector2Int(0, 0);
            Vector2Int goal = new Vector2Int(2, 2);
            
            // When: 경로 탐색
            List<Vector2Int> path = PathFinder.FindPath(start, goal, obstacles, 10, 10);
            
            // Then: 최단 경로 (맨해튼 거리 4)
            Assert.IsNotNull(path);
            Assert.AreEqual(5, path.Count); // 시작점 포함하여 5개 (0,0 -> 2,2)
        }
    }
}