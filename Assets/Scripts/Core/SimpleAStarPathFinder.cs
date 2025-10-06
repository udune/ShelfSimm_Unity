using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    // A* 알고리즘으로 최단 경로를 찾는 클래스
    public class SimpleAStarPathFinder : MonoBehaviour
    {
        [Header("격자 설정")] 
        public int gridWidth = 50; // 격자 가로 크기
        public int gridHeight = 50; // 격자 세로 크기
        
        private HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(); // 장애물 위치들 (갈 수 없는 칸들)

        private class Node // 한 칸의 정보를 담는 클래스
        {
            public Vector2Int position; // 현재 위치
            public int g; // 출발지부터 거리
            public int h; // 예상 거리
            public int f => g + h; // 총 점수
            public Node parent; // 이전 노드
        }

        private int CalculateDistance(Vector2Int from, Vector2Int to) // 맨해튼 거리 계산 (위아래좌우로만 이동)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y); // 예: (0,0)에서 (3,2)까지는 3+2=5칸
        }
        
        public void AddObstacle(Vector2Int pos) // 장애물 추가 (이 칸은 못 지나감)
        {
            obstacles.Add(pos); // 장애물 추가
        }
        
        public void RemoveObstacle(Vector2Int pos) // 장애물 제거
        {
            obstacles.Remove(pos); // 장애물 제거
        }

        private bool IsWalkable(Vector2Int pos) // 이 위치가 갈 수 있는 곳인지 확인
        {
            if (pos.x < 0 || pos.x >= gridWidth) // 격자 범위를 벗어나면 안됨
            {
                return false; // x 좌표 확인
            }
            
            if (pos.y < 0 || pos.y >= gridHeight) // 격자 범위를 벗어나면 안됨
            {
                return false; // y 좌표 확인
            }

            if (obstacles.Contains(pos)) // 장애물이 있으면 안됨
            {
                return false; // 장애물 확인
            }

            return true; // 갈 수 있는 곳
        }

        private readonly Vector2Int[] directions = new Vector2Int[] // 상하좌우 이동 방향
        {
            new Vector2Int(1, 0), // 오른쪽
            new Vector2Int(-1, 0), // 왼쪽
            new Vector2Int(0, 1), // 위쪽
            new Vector2Int(0, -1) // 아래쪽
        };

        private List<Vector2Int> GetNeighbors(Vector2Int pos) // 현재 위치의 이웃 노드들 가져오기
        {
            var neighbors = new List<Vector2Int>(); // 이웃 노드 리스트

            foreach (var dir in directions) // 각 방향에 대해
            {
                var neighbor = pos + dir; // 이웃 노드 위치 계산
                if (IsWalkable(neighbor)) // 이웃 노드가 갈 수 있는 곳이면
                {
                    neighbors.Add(neighbor); // 리스트에 추가
                }
            }
            
            return neighbors; // 이웃 노드 리스트 반환
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal) // A* 알고리즘으로 경로 찾기
        {
            if (!IsWalkable(start) && !IsWalkable(goal)) // 출발지와 목적지가 모두 갈 수 없는 곳이면
            {
                Debug.LogWarning($"[SimpleAStarPathfinder] 갈 수 없는 위치: start({start.x},{start.y}) goal({goal.x},{goal.y})");
                return null; // 경로 못 찾음
            }

            if (start == goal) // 출발지와 목적지가 같으면
            {
                Debug.Log($"[SimpleAStarPathfinder] 출발지와 목적지가 같음: ({start.x},{start.y})");
                return new List<Vector2Int> { start }; // 바로 반환
            }

            var openList = new List<Node>(); // 확인할 칸들
            var closedSet = new HashSet<Vector2Int>(); // 이미 확인한 칸들
            var allNodes = new Dictionary<Vector2Int, Node>(); // 모든 노드 정보
            
            var startNode = new Node // 시작 노드 만들기
            {
                position = start, // 위치
                g = 0, // 출발지부터 거리
                h = CalculateDistance(start, goal), // 예상 거리
                parent = null // 부모 노드 없음
            };
            
            openList.Add(startNode); // 시작 노드 열기 목록에 추가
            allNodes[start] = startNode; // 모든 노드에 시작 노드 추가

            while (openList.Count > 0) // 열기 목록에 노드가 남아있으면
            {
                var current = openList.OrderBy(n => n.f) // F 점수가 제일 낮은 노드 선택
                    .ThenBy(n => n.h) // F 점수가 같으면 H 점수가 낮은 노드 선택
                    .First(); // 제일 유망한 노드

                if (current.position == goal) // 목적지에 도착했으면
                {
                    Debug.Log($"[SimpleAStarPathfinder] 경로 찾음: start({start.x},{start.y}) goal({goal.x},{goal.y})");
                    return BuildPath(current); // 경로 만들기
                }

                openList.Remove(current); // 현재 노드를 열기 목록에서 제거
                closedSet.Add(current.position); // 현재 노드를 닫기 목록에 추가

                foreach (var neighborPos in GetNeighbors(current.position)) // 이웃 노드들에 대해
                {
                    if (closedSet.Contains(neighborPos)) // 이미 닫기 목록에 있으면 무시
                    {
                        continue; // 다음 이웃 노드로
                    }

                    int tentativeG = current.g + 1; // 현재 노드까지 거리 + 1 (이웃 노드까지 거리)

                    if (!allNodes.ContainsKey(neighborPos)) // 이웃 노드가 처음 발견된 노드이면
                    {
                        var neighborNode = new Node // 새 노드 만들기
                        {
                            position = neighborPos, // 위치
                            g = tentativeG, // 출발지부터 거리
                            h = CalculateDistance(neighborPos, goal), // 예상 거리
                            parent = current // 부모 노드 설정
                        };
                        allNodes[neighborPos] = neighborNode; // 모든 노드에 추가
                        openList.Add(neighborNode); // 열기 목록에 추가
                    }
                    else if (tentativeG < allNodes[neighborPos].g) // 이미 발견된 노드인데 더 짧은 경로를 찾았으면
                    {
                        var neighborNode = allNodes[neighborPos]; // 기존 노드 가져오기
                        neighborNode.g = tentativeG; // 출발지부터 거리 갱신
                        neighborNode.parent = current; // 부모 노드 갱신
                    }
                }
            }
            
            Debug.LogWarning($"[SimpleAStarPathfinder] 경로 찾기 실패: start({start.x},{start.y}) goal({goal.x},{goal.y})");
            return null; // 경로 못 찾음
        }

        private List<Vector2Int> BuildPath(Node endNode) // 노드에서 실제 경로 만들기 (거꾸로 따라가기)
        {
            var path = new List<Vector2Int>(); // 경로 리스트
            var current = endNode; // 현재 노드

            while (current != null) // 시작 노드까지 거슬러 올라가기
            {
                path.Add(current.position); // 현재 위치 추가
                current = current.parent; // 부모 노드로 이동
            }
            
            path.Reverse(); // 경로 뒤집기 (시작 → 끝 순서로)
            
            Debug.Log($"[SimpleAStarPathfinder] 경로 길이: {path.Count} 칸");
            return path; // 경로 반환
        }
    }
}
