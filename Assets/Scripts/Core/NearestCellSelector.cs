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
    
    public class NearestCellSelector : MonoBehaviour
    {
        [Header("설정")]
        [Range(1, 10)]
        public int topN = 3;

        [Header("참조")]
        public SimpleAStarPathFinder pathFinder;

        [Header("타이브레이커 설정")]
        public TiebreakerConfig tiebreakerConfig;
        private TiebreakerService tiebreakerService;

        private void Awake()
        {
            if (pathFinder == null)
            {
                pathFinder = GetComponent<SimpleAStarPathFinder>();
                if (pathFinder == null)
                {
                    Debug.LogWarning("[NearestCellSelector] SimpleAStarPathFinder를 찾을 수 없습니다!");
                }
            }

            if (tiebreakerConfig != null)
            {
                tiebreakerService = new TiebreakerService(tiebreakerConfig);
                DeterminismLogger.LogInitialization(tiebreakerConfig, tiebreakerConfig.randomSeed);
            }
            else
            {
                Debug.LogWarning("[NearestCellSelector] TiebreakerConfig가 설정되지 않았습니다. 타이브레이커 기능이 비활성화됩니다.");
            }
        }

        public int CalculateDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private List<CellDistanceInfo> SelectTopNCandidates(Vector2Int robotPos, List<CellDef> targetCells)
        {
            var candidates = new List<CellDistanceInfo>();

            foreach (var cell in targetCells)
            {
                var cellPos = new Vector2Int(cell.x, cell.y);
                var distance = CalculateDistance(robotPos, cellPos);

                candidates.Add(new CellDistanceInfo()
                {
                    cell = cell,
                    distance = distance,
                    actualPathCost = int.MaxValue,
                    path = null
                });
            }

            var sorted = candidates
                .OrderBy(c => c.distance)
                .ThenBy(c => c.cell.code)
                .Take(topN)
                .ToList();

            return sorted;
        }

        private List<CellDistanceInfo> RerankWithAStar(Vector2Int robotPos, List<CellDistanceInfo> candidates)
        {
            if (pathFinder == null)
            {
                Debug.LogWarning("[NearestCellSelector] SimpleAStarPathFinder가 설정되지 않았습니다. A* 재평가는 건너뜁니다.");
                return candidates;
            }

            var reranked = new List<CellDistanceInfo>();

            foreach (var candidate in candidates)
            {
                var cellPos = new Vector2Int(candidate.cell.x, candidate.cell.y);
                var path = pathFinder.FindPath(robotPos, cellPos);

                var updated = candidate;

                if (path != null && path.Count > 0)
                {
                    updated.actualPathCost = path.Count - 1;
                    updated.path = path;
                }
                else
                {
                    updated.actualPathCost = int.MaxValue;
                    updated.path = null;
                }

                reranked.Add(candidate);
            }

            List<CellDistanceInfo> finalList;

            if (tiebreakerService != null)
            {
                finalList = tiebreakerService.ApplyTiebreaker(reranked);
            }
            else
            {
                finalList = reranked
                    .OrderBy(c => c.actualPathCost)
                    .ThenBy(c => c.distance)
                    .ThenBy(c => c.cell.code)
                    .ToList();
            }

            return finalList;
        }
        
        public List<CellDistanceInfo> FilterTopN(Vector2Int robotPos, List<CellDef> targetCells)
        {
            if (targetCells == null || targetCells.Count == 0)
            {
                Debug.LogWarning("[NearestCellSelector] 대상 칸 목록이 비어 있습니다!");
                return new List<CellDistanceInfo>();
            }

            var candidates = SelectTopNCandidates(robotPos, targetCells);
            var finalList = RerankWithAStar(robotPos, candidates);

            var validCells = finalList
                .Where(c => c.actualPathCost < int.MaxValue)
                .ToList();

            return validCells;
        }

        public CellDistanceInfo? GetNearest(Vector2Int robotPos, List<CellDef> targetCells)
        {
            var topList = FilterTopN(robotPos, targetCells);
            if (topList.Count > 0)
            {
                return topList[0];
            }

            return null;
        }
    }
}
