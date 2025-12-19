using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    [Serializable]
    public struct CacheKey
    {
        public Vector2Int start;
        public Vector2Int goal;
        public string layoutHash;

        public CacheKey(Vector2Int start, Vector2Int goal, string layoutHash)
        {
            this.start = start;
            this.goal = goal;
            this.layoutHash = layoutHash;
        }

        public override int GetHashCode()
        {
            int hash = start.GetHashCode();
            hash = hash * 31 + goal.GetHashCode();
            hash = hash * 31 + (layoutHash?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is CacheKey other)
            {
                return start == other.start && goal == other.goal && layoutHash == other.layoutHash;
            }
            return false;
        }

        public override string ToString()
        {
            return $"({start.x}, {start.y}) -> ({goal.x}, {goal.y})";
        }
    }

    [Serializable]
    public struct CachedPath
    {
        public List<Vector2Int> path;
        public float cost;
        public bool success;
        public float cachedTime;

        public CachedPath(List<Vector2Int> path, float cost, bool success)
        {
            this.path = path;
            this.cost = cost;
            this.success = success;
            this.cachedTime = Time.realtimeSinceStartup;
        }
    }
    
    public class PathCache : MonoBehaviour
    {
        [Header("캐시 설정")]
        [SerializeField] private int maxCacheSize = 5000;

        [Range(0.1f, 5f)]
        [SerializeField] private float cleanupRatio = 0.2f;

        private Dictionary<CacheKey, CachedPath> cache;
        private string currentLayoutHash = "";

        private int hitCount = 0;
        private int missCount = 0;
        private int totalQueries = 0;

        private void Awake()
        {
            EnsureCacheInitialized();
        }

        private void EnsureCacheInitialized()
        {
            if (cache == null)
            {
                cache = new Dictionary<CacheKey, CachedPath>();
            }
        }

        public void SetLayoutHash(string layoutHash)
        {
            EnsureCacheInitialized();

            if (currentLayoutHash != layoutHash)
            {
                currentLayoutHash = layoutHash;
                InvalidateAll();
            }
        }

        public bool TryGet(Vector2Int start, Vector2Int goal, out CachedPath result)
        {
            EnsureCacheInitialized();

            totalQueries++;
            CacheKey key = new CacheKey(start, goal, currentLayoutHash);

            if (cache.TryGetValue(key, out result))
            {
                hitCount++;
                result.cachedTime = Time.realtimeSinceStartup;
                cache[key] = result;
                return true;
            }

            missCount++;
            return false;
        }

        public void Put(Vector2Int start, Vector2Int goal, CachedPath result)
        {
            EnsureCacheInitialized();

            CacheKey key = new CacheKey(start, goal, currentLayoutHash);

            if (cache.Count >= maxCacheSize && !cache.ContainsKey(key))
            {
                CleanOldEntries();
            }

            cache[key] = result;
        }

        private void CleanOldEntries()
        {
            int removeCount = Mathf.CeilToInt(maxCacheSize * cleanupRatio);

            var sortedEntries = new List<KeyValuePair<CacheKey, CachedPath>>(cache);
            sortedEntries.Sort((a, b) => a.Value.cachedTime.CompareTo(b.Value.cachedTime));

            for (int i = 0; i < removeCount && i < sortedEntries.Count; i++)
            {
                cache.Remove(sortedEntries[i].Key);
            }
        }

        public void InvalidateAll()
        {
            cache.Clear();
            hitCount = 0;
            missCount = 0;
            totalQueries = 0;
        }

        public void InvalidateEdges(List<(Vector2Int from, Vector2Int to)> edges)
        {
            if (edges == null || edges.Count == 0)
            {
                return;
            }

            var edgeSet = new HashSet<(Vector2Int, Vector2Int)>(edges);
            var keysToRemove = new List<CacheKey>();

            foreach (var keyValue in cache)
            {
                if (keyValue.Value.path == null || keyValue.Value.path.Count < 2)
                {
                    continue;
                }

                for (int i = 0; i < keyValue.Value.path.Count - 1; i++)
                {
                    var edge = (keyValue.Value.path[i], keyValue.Value.path[i + 1]);
                    if (edgeSet.Contains(edge))
                    {
                        keysToRemove.Add(keyValue.Key);
                        break;
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }

        public void InvalidateRegion(List<Vector2Int> affectedCells)
        {
            if (affectedCells == null || affectedCells.Count == 0)
            {
                return;
            }

            var keysToRemove = new List<CacheKey>();
            var cellSet = new HashSet<Vector2Int>(affectedCells);

            foreach (var keyValue in cache)
            {
                if (cellSet.Contains(keyValue.Key.start) || cellSet.Contains(keyValue.Key.goal))
                {
                    keysToRemove.Add(keyValue.Key);
                    continue;
                }

                if (keyValue.Value.path != null)
                {
                    foreach (var path in keyValue.Value.path)
                    {
                        if (cellSet.Contains(path))
                        {
                            keysToRemove.Add(keyValue.Key);
                            break;
                        }
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }

        public int GetCacheSize()
        {
            return cache.Count;
        }

        public float GetHitRate()
        {
            if (totalQueries == 0)
            {
                return 0.0f;
            }
            return (float)hitCount / totalQueries;
        }

        public string GetStatistics()
        {
            return $"캐시 크기: {GetCacheSize()}, 적중률: {GetHitRate():P2}, 총 조회: {totalQueries}, 적중: {hitCount}, 미스: {missCount}";
        }

        private void OnValidate()
        {
            if (maxCacheSize < 100)
            {
                maxCacheSize = 100;
            }

            if (cleanupRatio < 0.1f)
            {
                cleanupRatio = 0.1f;
            }

            if (cleanupRatio > 0.5f)
            {
                cleanupRatio = 0.5f;
            }
        }
    }
}
