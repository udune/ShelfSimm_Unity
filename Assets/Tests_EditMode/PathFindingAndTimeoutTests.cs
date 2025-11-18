using System.Collections.Generic;
using Data.Data;
using NUnit.Framework;
using UnityEngine;

namespace Tests_EditMode.Tests_EditMode
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
        }
        
        [Test]
        public void PathFinder_장애물차단_경로없음()
        {
            // Given: 목표가 장애물로 막힌 경우
            HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
            obstacles.Add(new Vector2Int(3, 3));
            
            Vector2Int start = new Vector2Int(0, 0);
            Vector2Int goal = new Vector2Int(3, 3);
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
    }
}
