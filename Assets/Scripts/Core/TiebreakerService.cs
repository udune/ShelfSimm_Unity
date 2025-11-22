using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class TiebreakerService
    {
        private readonly TiebreakerConfig config;
        private System.Random random;

        public TiebreakerService(TiebreakerConfig config)
        {
            this.config = config;
            InitializeRandom(config.randomSeed);
        }

        public void InitializeRandom(int seed)
        {
            random = new System.Random(seed);

            if (config.enableLogging)
            {
                Debug.Log($"[TiebreakerService] Random initialized with seed {seed}");
            }
        }

        public List<CellDistanceInfo> ApplyTiebreaker(List<CellDistanceInfo> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return candidates;
            }

            var groups = candidates
                .GroupBy(c => c.actualPathCost)
                .OrderBy(g => g.Key)
                .ToList();

            var result = new List<CellDistanceInfo>();

            foreach (var group in groups)
            {
                var groupList = group.ToList();

                if (groupList.Count == 1)
                {
                    result.Add(groupList[0]);
                }
                else
                {
                    var sorted = ApplyTiebreakerToGroup(groupList);
                    result.AddRange(sorted);
                }
            }

            return result;
        }

        private List<CellDistanceInfo> ApplyTiebreakerToGroup(List<CellDistanceInfo> group)
        {
            switch (config.mode)
            {
                case TiebreakerConfig.TiebreakerMode.Alphabetical:
                    return ApplyAlphabeticalTiebreaker(group);
                case TiebreakerConfig.TiebreakerMode.Random:
                    return ApplyRandomTiebreaker(group);
                default:
                    return group;
            }
        }

        private List<CellDistanceInfo> ApplyAlphabeticalTiebreaker(List<CellDistanceInfo> group)
        {
            var sorted = group.OrderBy(c => c.cell.code).ToList();

            if (config.enableLogging && group.Count > 1)
            {
                var codes = string.Join(", ", sorted.Select(c => c.cell.code));
                Debug.Log($"[TiebreakerService] Alphabetical tiebreaker applied: {codes}");
            }

            return sorted;
        }

        private List<CellDistanceInfo> ApplyRandomTiebreaker(List<CellDistanceInfo> group)
        {
            var shuffled = new List<CellDistanceInfo>(group);
            int n = shuffled.Count;

            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            if (config.enableLogging && group.Count > 1)
            {
                var codes = string.Join(", ", shuffled.Select(c => c.cell.code));
                Debug.Log($"[TiebreakerService] Random tiebreaker applied: {codes}");
            }

            return shuffled;
        }
    }
}