using UnityEngine;

namespace Core
{
    // 결정론적 로그 출력 클래스
    public static class DeterminismLogger
    {
        // 로그 초기화 정보
        public static void LogInitialization(TiebreakerConfig config, int randomSeed)
        {
            Debug.Log("=== Deterministic Initialization ===");
            Debug.Log($"tiebreak={config.mode.ToString().ToLower()} seed={randomSeed}");
            Debug.Log($"deterministic_init {{pathfinder: true, job_order: true, spawn: true}}");
            Debug.Log("===================================");
        }

        // 로그 랜덤 시드 변경 정보
        public static void LogSeedChange(int oldSeed, int newSeed)
        {
            Debug.Log($"[DeterminismLogger] Random seed changed from {oldSeed} to {newSeed}");
        }

        // 로그 캐시 상태 정보
        public static void LogCacheState(int hits, int misses)
        {
            float hitRate = hits + misses > 0 ? (float) hits / (hits + misses) : 0; // 히트율 계산
            Debug.Log($"[DeterminismLogger] Cache Hits: {hits}, Misses: {misses}, Hit Rate: {hitRate:F2}%");
        }
    }
}