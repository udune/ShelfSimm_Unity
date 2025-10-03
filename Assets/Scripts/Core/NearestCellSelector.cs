using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Core
{
    // 칸과 거리 정보를 함께 담는 구조체
    [Serializable]
    public struct CellDistanceInfo
    {
        public CellDef cell; // 칸 정보
        public int distance; // 맨해튼 거리
        public int actualPathCost; // A* 실제 경로 비용
        public List<Vector2Int> path; // 실제 경로
    }
    
    // 로봇 위치에서 가장 가까운 N개의 칸을 선택하는 기능
    public class NearestCellSelector : MonoBehaviour
    {
        [Header("설정")] 
        [Range(1, 10)] 
        public int topN = 3; // 선택할 칸의 개수

        [Header("참조")] 
        public SimpleAStarPathFinder pathFinder; // A* 경로 탐색기

        private void Awake() // 초기화
        {
            if (pathFinder == null) // pathFinder가 없으면 자동으로 찾기
            {
                pathFinder = GetComponent<SimpleAStarPathFinder>(); // 같은 게임 오브젝트에서 찾기
                if (pathFinder == null) // 그래도 없으면 경고
                {
                    Debug.LogWarning("[NearestCellSelector] SimpleAStarPathFinder를 찾을 수 없습니다!");
                }
            }
        }

        // 맨해튼 거리 계산
        // 공식: |x1 - x2| + |y1 - y2|
        public int CalculateDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y); // 맨해튼 거리 계산
        }

        private List<CellDistanceInfo> SelectTopNCandidates(Vector2Int robotPos, List<CellDef> targetCells) // 맨해튼 거리로 TopN개 후보 선택
        {
            var candidates = new List<CellDistanceInfo>(); // 후보 리스트

            foreach (var cell in targetCells) // 각 칸에 대해
            {
                var cellPos = new Vector2Int(cell.x, cell.y); // 칸 위치
                var distance = CalculateDistance(robotPos, cellPos); // 로봇과 칸 사이의 맨해튼 거리 계산
                
                candidates.Add(new CellDistanceInfo() // 후보 추가
                {
                    cell = cell, // 칸 정보
                    distance = distance, // 맨해튼 거리
                    actualPathCost = int.MaxValue, // A* 실제 경로 비용 (초기값)
                    path = null // 실제 경로 (초기값)
                });
            }

            var sorted = candidates // 거리순, 코드순으로 정렬 후 상위 N개 선택
                .OrderBy(c => c.distance) // 거리 오름차순
                .ThenBy(c => c.cell.code) // 코드 오름차순
                .Take(topN) // 상위 N개 선택
                .ToList(); // 리스트로 변환
            
            Debug.Log($"[NearestCellSelector] 1단계: 맨해튼 거리로 상위 {sorted.Count}개 선택");
            for (int i = 0; i < sorted.Count; i++) // 결과 출력
            {
                var info = sorted[i]; // 각 칸 정보
                Debug.Log($"{i + 1}. {info.cell.code} at ({info.cell.x}, {info.cell.y}) - 맨해튼 거리: {info.distance}");
            }

            return sorted; // 상위 N개 칸 반환
        }

        private List<CellDistanceInfo> RerankWithAStar(Vector2Int robotPos, List<CellDistanceInfo> candidates) // A*로 실제 경로 비용 재평가 및 정렬
        {
            if (pathFinder == null) // pathFinder가 없으면 경고 후 원래 후보 반환
            {
                Debug.LogWarning("[NearestCellSelector] SimpleAStarPathFinder가 설정되지 않았습니다. A* 재평가는 건너뜁니다.");
                return candidates; // 원래 후보 반환
            }
            
            var reranked = new List<CellDistanceInfo>(); // 재평가된 후보 리스트

            foreach (var candidate in candidates) // 각 후보에 대해
            {
                var cellPos = new Vector2Int(candidate.cell.x, candidate.cell.y); // 칸 위치
                
                var path = pathFinder.FindPath(robotPos, cellPos); // A*로 실제 경로 찾기

                var updated = candidate; // 후보 복사

                if (path != null && path.Count > 0) // 경로가 있으면
                {
                    updated.actualPathCost = path.Count - 1; // 실제 경로 비용 (칸 수 - 1)
                    updated.path = path; // 실제 경로 저장
                    Debug.Log($"  A* 평가: {candidate.cell.code} - 실제 경로: {updated.actualPathCost}칸");
                }
                else
                {
                    updated.actualPathCost = int.MaxValue; // 경로 없음 표시
                    updated.path = null; // 경로 없음
                    Debug.LogWarning($"  A* 평가: {candidate.cell.code} - 경로 없음!");
                }
                
                reranked.Add(candidate); // 재평가된 후보 추가
            }

            var sorted = reranked // 실제 경로 비용, 거리, 코드순으로 정렬
                .OrderBy(c => c.actualPathCost) // 실제 경로 비용 오름차순
                .ThenBy(c => c.distance) // 맨해튼 거리 오름차순
                .ThenBy(c => c.cell.code) // 코드 오름차순
                .ToList(); // 리스트로 변환
            
            Debug.Log($"[NearestCellSelector] A* 실제 경로 비용으로 재평가 후 정렬");
            for (int i = 0; i < sorted.Count; i++) // 결과 출력
            {
                var info = sorted[i]; // 각 칸 정보
                if (info.actualPathCost < int.MaxValue) // 실제 경로가 있으면
                {
                    Debug.Log($"최종 {i+1}. {info.cell.code} at ({info.cell.x}, {info.cell.y}) - 실제 경로 비용: {info.actualPathCost}칸, 맨해튼 거리: {info.distance}");
                }
                else // 실제 경로가 없으면
                {
                    Debug.Log($"최종 {i+1}. {info.cell.code} at ({info.cell.x}, {info.cell.y}) - 실제 경로 없음, 맨해튼 거리: {info.distance}");
                }
            }
            
            return sorted; // 재평가 및 정렬된 후보 반환
        }
        
        // 로봇 위치와 칸 목록을 받아 가장 가까운 N개의 칸을 거리순으로 반환
        public List<CellDistanceInfo> FilterTopN(Vector2Int robotPos, List<CellDef> targetCells)
        {
            Debug.Log($"[NearestCellSelector] FilterTopN 호출 - 로봇 위치: ({robotPos.x}, {robotPos.y}), 대상 칸 수: {targetCells.Count}");
            Debug.Log($"[NearestCellSelector] 설정된 topN 값: {topN}");
            
            if (targetCells == null || targetCells.Count == 0) // 대상 칸이 없으면
            {
                Debug.LogWarning("[NearestCellSelector] 대상 칸 목록이 비어 있습니다!");
                return new List<CellDistanceInfo>(); // 빈 리스트 반환
            }

            var candidates = SelectTopNCandidates(robotPos, targetCells); // 맨해튼 거리로 TopN 후보 선택
            
            var finalList = RerankWithAStar(robotPos, candidates); // A*로 실제 경로 비용 재평가 및 정렬

            var validCells = finalList // 실제 경로가 있는 칸만 필터링
                .Where(c => c.actualPathCost < int.MaxValue) // 실제 경로가 있는 칸만
                .ToList(); // 리스트로 변환
            
            Debug.Log($"[NearestCellSelector] 최종 유효 칸 수: {validCells.Count}");
            return validCells; // 최종 유효 칸 반환
        }

        public CellDistanceInfo? GetNearest(Vector2Int robotPos, List<CellDef> targetCells) // 가장 가까운 단일 칸 선택
        {
            var topList = FilterTopN(robotPos, targetCells); // TopN 칸 리스트 가져오기
            if (topList.Count > 0) // 하나 이상 있으면
            {
                return topList[0]; // 가장 가까운 칸 반환
            }

            return null; // 없으면 null 반환
        }
    }
}
