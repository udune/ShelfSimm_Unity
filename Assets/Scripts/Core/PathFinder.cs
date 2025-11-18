using System.Collections.Generic;
using UnityEngine;

namespace Core.Core
{
    public static class PathFinder
    {
        public class Node
        {
            public Vector2Int pos; // 노드 위치
            public float g; // 시작점에서 현재 노드까지의 비용
            public float h; // 현재 노드에서 목표 노드까지의 추정 비용
            public float f => g + h; // 총 비용 (f = g + h)
            public Node parent; // 부모 노드
        }

        // A* 알고리즘을 사용하여 시작점에서 목표점까지의 경로를 찾는 메서드
        public static List<Vector2Int> FindPath(
            Vector2Int start, // 시작점
            Vector2Int goal, // 목표점
            HashSet<Vector2Int> obstacles, // 장애물 위치 집합
            int maxWidth, // 맵 최대 너비
            int maxHeight) // 맵 최대 높이
        {
            if (start == goal) // 시작점과 목표점이 같으면 빈 경로 반환
            {
                return new List<Vector2Int>() { start }; // 시작점 포함
            }

            if (obstacles.Contains(goal)) // 목표점이 장애물에 있으면 null 반환
            {
                return null;
            }
            
            List<Node> openList = new List<Node>(); // 탐색할 노드 리스트
            HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>(); // 이미 탐색한 노드 집합
            
            Node startNode = new Node // 시작 노드 생성
            {
                pos = start,
                g = 0,
                h = Heuristic(start, goal),
                parent = null
            };
            openList.Add(startNode); // 시작 노드를 오픈 리스트에 추가

            while (openList.Count > 0) // 오픈 리스트에 노드가 남아있으면 계속 탐색
            {
                Node current = GetLowestF(openList);
                openList.Remove(current); // 현재 노드를 오픈 리스트에서 제거
                
                if (current.pos == goal) // 목표점에 도달하면 경로 생성
                {
                    return ReconstructPath(current); // 경로 재구성
                }
                
                closedSet.Add(current.pos); // 현재 노드를 클로즈드 셋에 추가
                
                Vector2Int[] neighbors = new Vector2Int[]
                {
                    new Vector2Int(0, 1), // 위
                    new Vector2Int(1, 0), // 오른쪽
                    new Vector2Int(0, -1), // 아래
                    new Vector2Int(-1, 0) // 왼쪽
                };

                foreach (var dir in neighbors) // 모든 이웃 노드 검사
                {
                    Vector2Int neighborPos = current.pos + dir; // 이웃 노드 위치 계산
                    
                    if (neighborPos.x < 0 || neighborPos.x >= maxWidth || 
                        neighborPos.y < 0 || neighborPos.y >= maxHeight)
                    {
                        continue; // 맵 경계를 벗어나면 무시
                    }

                    if (obstacles.Contains(neighborPos)) // 장애물이면 무시
                    {
                        continue;
                    }
                    
                    if (closedSet.Contains(neighborPos))
                    {
                        continue; // 이미 탐색한 노드면 무시
                    }
                    
                    float tentativeG = current.g + 1; // 이동 비용 (가로세로 이동만 고려)
                    
                    Node existingNode = openList.Find(x => x.pos == neighborPos); // 오픈 리스트에서 이웃 노드 찾기
                    if (existingNode == null) // 새로운 노드면
                    {
                        Node newNode = new Node // 새로운 노드 생성
                        {
                            pos = neighborPos,
                            g = tentativeG,
                            h = Heuristic(neighborPos, goal),
                            parent = current
                        };
                        openList.Add(newNode); // 새로운 노드를 오픈 리스트에 추가
                    }
                    else if (tentativeG < existingNode.g) // 더 나은 경로 발견 시
                    {
                        existingNode.g = tentativeG; // 더 나은 경로 발견 시 비용 갱신
                        existingNode.parent = current; // 부모 노드 갱신
                    }
                }
            }

            return null;
        }

        // f 값이 가장 낮은 노드를 오픈 리스트에서 찾는 메서드
        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 맨해튼 거리 계산
        }

        // 경로를 재구성하는 메서드
        private static Node GetLowestF(List<Node> nodes)
        {
            Node lowest = nodes[0]; // 초기값 설정
            for (int i = 0; i < nodes.Count; i++) // 모든 노드를 검사
            {
                if (nodes[i].f < lowest.f) // f 값이 더 낮으면 갱신
                {
                    lowest = nodes[i]; // 최저 노드 갱신
                }
            }
            
            return lowest; // 최저 노드 반환
        }
        
        // 경로를 재구성하는 메서드
        private static List<Vector2Int> ReconstructPath(Node endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>(); // 경로 리스트 초기화
            Node current = endNode;
            while (current != null) // 부모 노드가 없을 때까지 반복
            {
                path.Add(current.pos); // 현재 노드 위치를 경로에 추가
                current = current.parent; // 부모 노드로 이동
            }
            path.Reverse(); // 경로를 시작점에서 목표점 순서로 뒤집기
            return path; // 경로 반환
        }
    }
}