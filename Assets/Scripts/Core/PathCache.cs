using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    // 캐시 키 구조체
    [Serializable]
    public struct CacheKey
    {
        public Vector2Int start; // 시작 지점
        public Vector2Int goal; // 목표 지점

        // 생성자
        public CacheKey(Vector2Int start, Vector2Int goal)
        {
            this.start = start; // 시작 지점
            this.goal = goal; // 목표 지점
        }

        // 해시 코드 계산
        public override int GetHashCode()
        {
            int startHash = start.GetHashCode(); // start의 해시 코드 계산
            int goalHash = goal.GetHashCode(); // goal의 해시 코드 계산

            return startHash + goalHash * 31; // 31은 소수로, 해시 충돌을 줄이기 위해 사용
        }

        // 동등성 비교
        public override bool Equals(object obj)
        {
            if (obj is CacheKey other) // obj가 CacheKey 타입인지 확인
            {
                return start == other.start && goal == other.goal; // start와 goal이 모두 동일한지 비교
            }

            return false; // obj가 CacheKey 타입이 아니면 false 반환
        }

        // 문자열 표현
        public override string ToString()
        {
            return $"({start.x}, {start.y}) -> ({goal.x}, {goal.y})"; // 캐시 키를 문자열로 표현
        }
    }
    
    // 캐시 키 구조체
    [Serializable]
    public struct CachedPath
    {
        public List<Vector2Int> path; // 경로
        public float cost; // 경로 비용
        public bool success; // 경로 탐색 성공 여부
        public float cachedTime; // 캐시된 시간
        
        public CachedPath(List<Vector2Int> path, float cost, bool success) // 생성자
        {
            this.path = path; // 경로
            this.cost = cost; // 비용
            this.success = success; // 성공 여부
            this.cachedTime = Time.realtimeSinceStartup; // 캐시된 시간
        }
    }
    
    public class PathCache : MonoBehaviour
    {
        [Header("캐시 설정")]
        [SerializeField]
        private int maxCacheSize = 5000; // 최대 캐시 크기
        
        [Range(0.1f, 5f)]
        [SerializeField]
        private float cleanupRatio = 0.2f; // 캐시 정리 비율

        [Header("디버그")] 
        [SerializeField] 
        private bool enableDetailedLogging = true; // 상세 로그 활성화 여부

        [SerializeField] 
        private bool showStatistics = true; // 통계 표시 여부

        private Dictionary<CacheKey, CachedPath> cache; // 캐시 저장소

        private int hitCount = 0; // 캐시 적중 횟수
        private int missCount = 0; // 캐시 미스 횟수
        private int totalQueries = 0; // 총 조회 횟수

        private float lastGCLogTime = 0f; // 마지막 GC 로그 출력 시간
        private const float GCLogInterval = 60f; // 60초마다 GC 로그 출력

        private void Awake()
        {
            // 캐시 초기화
            cache = new Dictionary<CacheKey, CachedPath>();

            // 초기화 로그 출력
            if (enableDetailedLogging)
            {
                Debug.Log($"[PathCache] 초기화 완료 - 최대 크기: {maxCacheSize}");
            }
        }

        public bool TryGet(Vector2Int start, Vector2Int goal, out CachedPath result) // 캐시에서 경로를 조회
        {
            totalQueries++; // 총 조회 횟수 증가
            CacheKey key = new CacheKey(start, goal); // 캐시 키 생성

            if (cache.TryGetValue(key, out result)) // 캐시에서 경로 조회 시도
            {
                hitCount++; // 캐시 적중 횟수 증가
                
                if (enableDetailedLogging) // 상세 로그 출력
                {
                    Debug.Log($"[PathCache] 캐시 적중: {key} (비용: {result.cost}, 성공: {result.success})");
                }
                
                result.cachedTime = Time.realtimeSinceStartup; // 캐시된 시간 갱신
                cache[key] = result; // 갱신된 경로로 캐시 업데이트

                return true; // 경로 반환
            }
            
            missCount++; // 캐시 미스 횟수 증가
            
            if (enableDetailedLogging) // 상세 로그 출력
            {
                Debug.Log($"[PathCache] 캐시 미스: {key}"); // 캐시 미스 로그 출력
            }

            return false; // 경로 없음
        }

        public void Put(Vector2Int start, Vector2Int goal, CachedPath result) // 캐시에 경로 저장
        {
            CacheKey key = new CacheKey(start, goal); // 캐시 키 생성

            if (cache.Count >= maxCacheSize && !cache.ContainsKey(key)) // 캐시 크기 초과 시 오래된 항목 제거
            {
                CleanOldEntries(); // 오래된 항목 제거
            }
            
            cache[key] = result; // 캐시에 경로 저장

            if (enableDetailedLogging) // 상세 로그 출력
            {
                Debug.Log($"[PathCache] 경로 캐시 저장: {key} (비용: {result.cost}, 성공: {result.success})");
            }
        }

        private void CleanOldEntries() // 오래된 항목 제거
        {
            int beforeCount = cache.Count; // 제거 전 캐시 크기
            int removeCount = Mathf.CeilToInt(maxCacheSize * cleanupRatio); // 제거할 항목 수

            var sortedEntries = new List<KeyValuePair<CacheKey, CachedPath>>(cache); // 캐시 항목 리스트 생성
            sortedEntries.Sort((a, b) => a.Value.cachedTime.CompareTo(b.Value.cachedTime)); // 캐시된 시간 기준으로 정렬

            for (int i = 0; i < removeCount && i < sortedEntries.Count; i++) // 오래된 항목 제거
            {
                cache.Remove(sortedEntries[i].Key); // 캐시에서 항목 제거
            }

            int afterCount = cache.Count; // 제거 후 캐시 크기
            int evicted = beforeCount - afterCount; // 제거된 항목 수
            
            if (Time.realtimeSinceStartup - lastGCLogTime >= GCLogInterval) // 주기적으로 GC 로그 출력
            {
                lastGCLogTime = Time.realtimeSinceStartup; // 마지막 로그 시간 갱신
                if (enableDetailedLogging) // 상세 로그 출력
                {
                    Debug.Log($"[PathCache] 캐시 정리 완료 - 제거된 항목: {evicted}, 현재 크기: {afterCount}");   
                }
            }
        }

        public void InvalidateAll() // 모든 캐시 무효화
        {
            int oldCount = cache.Count; // 이전 캐시 크기
            cache.Clear(); // 캐시 초기화
            
            hitCount = 0; // 캐시 적중 횟수 초기화
            missCount = 0; // 캐시 미스 횟수 초기화
            totalQueries = 0; // 총 조회 횟수 초기화
            
            if (enableDetailedLogging) // 상세 로그 출력
            {
                Debug.Log($"[PathCache] 모든 캐시 무효화 - 이전 크기: {oldCount}");
            }
        }

        public void InvalidateRegion(List<Vector2Int> affectedCells) // 특정 영역 무효화
        {
            if (affectedCells == null || affectedCells.Count == 0) // 무효화할 셀이 없으면 반환
            {
                return; // 무효화할 셀이 없으면 반환
            }
            
            var keysToRemove = new List<CacheKey>(); // 제거할 캐시 키 리스트
            var cellSet = new HashSet<Vector2Int>(affectedCells); // 영향을 받는 셀 집합

            foreach (var keyValue in cache) // 캐시 항목 순회
            {
                if (cellSet.Contains(keyValue.Key.start) || cellSet.Contains(keyValue.Key.goal)) // 시작점이나 목표점이 영향을 받는 셀에 포함되면 제거
                {
                    keysToRemove.Add(keyValue.Key); // 제거할 키에 추가
                    continue; // 다음 항목으로
                }

                if (keyValue.Value.path != null) // 경로가 null이 아니면
                {
                    foreach (var path in keyValue.Value.path) // 경로의 각 지점 순회
                    {
                        if (cellSet.Contains(path)) // 경로 지점이 영향을 받는 셀에 포함되면 제거
                        {
                            keysToRemove.Add(keyValue.Key); // 제거할 키에 추가
                            break; // 다음 캐시 항목으로
                        }
                    }
                }
            }
            
            foreach (var key in keysToRemove) // 제거할 키 순회
            {
                cache.Remove(key); // 캐시에서 키 제거
            }
            
            if (keysToRemove.Count > 0) { // 제거된 항목이 있으면 로그 출력
                if (enableDetailedLogging) // 상세 로그 출력
                {
                    Debug.Log($"[PathCache] 영역 무효화 - 제거된 항목: {keysToRemove.Count}, 현재 크기: {cache.Count}");
                }
            }
        }

        public int GetCacheSize() // 현재 캐시 크기 반환
        {
            return cache.Count; // 캐시 크기 반환
        }

        public float GetHitRate() // 캐시 적중률 반환
        {
            if (totalQueries == 0) // 조회가 없으면 0 반환
            {
                return 0.0f;
            }

            return (float)hitCount / totalQueries; // 적중률 계산하여 반환
        }

        public string GetStatistics() // 캐시 통계 문자열 반환
        {
            return $"캐시 크기: {GetCacheSize()}, 적중률: {GetHitRate():P2}, 총 조회: {totalQueries}, 적중: {hitCount}, 미스: {missCount}";
        }

        private void OnGUI() // 통계 정보 GUI 표시
        {
            if (!showStatistics) // 통계 표시 비활성화 시 반환
            {
                return;
            }

            int width = 300; // 박스 너비
            int height = 100; // 박스 높이
            int margin = 10; // 화면 가장자리에서의 여백
            
            Rect rect = new Rect(Screen.width - width - margin, margin, width, height); // 통계 박스 위치 및 크기 설정

            GUI.color = new Color(0, 0, 0, 0.7f); // 반투명 검정색
            GUI.Box(rect, ""); // 배경 박스
            
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label) // 스타일 설정
            {
                fontSize = 12, // 글꼴 크기
                normal =
                {
                    textColor = Color.white // 글꼴 색상
                },
                padding = new RectOffset(10, 10, 10, 10) // 패딩 설정
            };

            string stats = $"<b>PathCache 통계</b>\n" +
                           $"크기: {cache.Count}/{maxCacheSize}\n" +
                           $"적중률: {GetHitRate():P2}\n" +
                           $"HIT: {hitCount} / MISS: {missCount}"; // 통계 정보 문자열
            
            GUI.Label(rect, stats, style); // 통계 정보 표시
            GUI.color = Color.white; // GUI 색상 초기화
        }

        private void OnValidate() // 인스펙터 값 변경 시 호출
        {
            if (maxCacheSize < 100) // 최소 캐시 크기 제한
            {
                maxCacheSize = 100; // 최소값 설정
                Debug.LogWarning("[PathCache] 최대 캐시 크기는 최소 100이어야 합니다.");
            }

            if (cleanupRatio < 0.1f) // 최소 정리 비율 제한
            {
                cleanupRatio = 0.1f; // 최소값 설정
            }
            
            if (cleanupRatio > 0.5f) // 최대 정리 비율 제한
            {
                cleanupRatio = 0.5f; // 최대값 설정
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected() // 선택된 상태에서 경로 시각화
        {
            if (cache == null || cache.Count == 0) // 캐시가 비어있으면 반환
            {
                return;
            }

            Gizmos.color = Color.cyan; // 경로 색상 설정
            int displayCount = 0; // 표시된 경로 수
            int maxDisplay = 10; // 최대 표시할 경로 수

            foreach (var keyValue in cache) // 캐시 항목 순회
            {
                if (displayCount >= maxDisplay) // 최대 표시 경로 수 초과 시 종료
                {
                    break;
                }
                
                if (keyValue.Value.path != null && keyValue.Value.path.Count > 1) // 경로가 유효하면 시각화
                {
                    for (int i = 0; i < keyValue.Value.path.Count - 1; i++) // 경로의 각 지점 순회
                    {
                        Vector3 from = new Vector3(keyValue.Value.path[i].x, 0.1f, keyValue.Value.path[i].y); // 시작 지점
                        Vector3 to = new Vector3(keyValue.Value.path[i + 1].x, 0.1f, keyValue.Value.path[i + 1].y); // 목표 지점
                        Gizmos.DrawLine(from, to); // 경로 선 그리기
                    }

                    displayCount++; // 표시된 경로 수 증가
                }
            }
        }
#endif
    }
}
