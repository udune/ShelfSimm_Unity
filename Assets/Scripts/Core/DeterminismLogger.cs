using UnityEngine;

namespace Core
{
    public static class DeterminismLogger
    {
        public static void LogInitialization(TiebreakerConfig config, int randomSeed)
        {
            Debug.Log($"=== Deterministic Init: tiebreak={config.mode.ToString().ToLower()} seed={randomSeed} ===");
        }

        public static void LogSeedChange(int oldSeed, int newSeed)
        {
            Debug.Log($"Random seed changed: {oldSeed} -> {newSeed}");
        }

        public static void LogCacheState(int hits, int misses)
        {
            float hitRate = hits + misses > 0 ? (float)hits / (hits + misses) : 0;
            Debug.Log($"Cache - Hits: {hits}, Misses: {misses}, Rate: {hitRate:F2}%");
        }
    }
}
