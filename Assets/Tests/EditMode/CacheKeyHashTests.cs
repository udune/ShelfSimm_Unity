using NUnit.Framework;
using UnityEngine;
using Core;

namespace Tests.EditMode
{
    /// <summary>
    /// CacheKey에 layout_hash 포함 검증 테스트
    /// T-205의 핵심 기능 검증
    /// </summary>
    [TestFixture]
    public class CacheKeyHashTests
    {
        #region CacheKey 구조체 테스트

        [Test]
        [Description("동일한 start, goal, layoutHash는 동일한 CacheKey")]
        public void CacheKey_SameValues_AreEqual()
        {
            // Given
            var key1 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash123");
            var key2 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash123");
            
            // Then
            Assert.AreEqual(key1, key2, "동일한 값을 가진 CacheKey는 같아야 함");
            Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode(), "해시코드도 동일해야 함");
        }

        [Test]
        [Description("다른 layoutHash는 다른 CacheKey")]
        public void CacheKey_DifferentLayoutHash_AreNotEqual()
        {
            // Given: start, goal은 같지만 layoutHash가 다름
            var key1 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash123");
            var key2 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash456");
            
            // Then
            Assert.AreNotEqual(key1, key2, "layoutHash가 다르면 다른 CacheKey여야 함");
        }

        [Test]
        [Description("다른 start는 다른 CacheKey")]
        public void CacheKey_DifferentStart_AreNotEqual()
        {
            // Given
            var key1 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash123");
            var key2 = new CacheKey(new Vector2Int(1, 1), new Vector2Int(5, 5), "hash123");
            
            // Then
            Assert.AreNotEqual(key1, key2, "start가 다르면 다른 CacheKey여야 함");
        }

        [Test]
        [Description("다른 goal은 다른 CacheKey")]
        public void CacheKey_DifferentGoal_AreNotEqual()
        {
            // Given
            var key1 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), "hash123");
            var key2 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(6, 6), "hash123");
            
            // Then
            Assert.AreNotEqual(key1, key2, "goal이 다르면 다른 CacheKey여야 함");
        }

        [Test]
        [Description("null layoutHash 처리")]
        public void CacheKey_NullLayoutHash_WorksCorrectly()
        {
            // Given
            var key1 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), null);
            var key2 = new CacheKey(new Vector2Int(0, 0), new Vector2Int(5, 5), null);
            
            // Then
            Assert.AreEqual(key1, key2, "null layoutHash도 동일하게 처리되어야 함");
        }

        [Test]
        [Description("CacheKey ToString은 읽기 쉬운 형식")]
        public void CacheKey_ToString_IsReadable()
        {
            // Given
            var key = new CacheKey(new Vector2Int(3, 4), new Vector2Int(10, 20), "hash123");
            
            // When
            string result = key.ToString();
            
            // Then
            Assert.IsTrue(result.Contains("3"), "start x 좌표가 포함되어야 함");
            Assert.IsTrue(result.Contains("4"), "start y 좌표가 포함되어야 함");
            Assert.IsTrue(result.Contains("10"), "goal x 좌표가 포함되어야 함");
            Assert.IsTrue(result.Contains("20"), "goal y 좌표가 포함되어야 함");
        }

        #endregion

        #region PathCache와 CacheKey 통합 테스트

        [Test]
        [Description("AC-6.3: 동일 경로라도 layoutHash가 다르면 별도 캐시")]
        public void PathCache_DifferentLayoutHash_SeparateCacheEntries()
        {
            // Given
            var pathCache = CreatePathCache();
            var start = new Vector2Int(0, 0);
            var goal = new Vector2Int(5, 5);
            
            // 첫 번째 레이아웃 해시로 캐시 설정
            pathCache.SetLayoutHash("hash_v1");
            var path1 = new CachedPath(
                new System.Collections.Generic.List<Vector2Int> { start, goal }, 
                10.0f, 
                true
            );
            pathCache.Put(start, goal, path1);
            
            // 두 번째 레이아웃 해시로 변경 (전역 무효화됨)
            pathCache.SetLayoutHash("hash_v2");
            
            // When: 같은 start, goal로 조회
            bool found = pathCache.TryGet(start, goal, out _);
            
            // Then: 찾을 수 없음 (layoutHash가 달라서)
            Assert.IsFalse(found, "layoutHash가 다르면 이전 캐시를 찾을 수 없어야 함");
        }

        [Test]
        [Description("AC-6.3: 동일 layoutHash로는 캐시 재사용 가능")]
        public void PathCache_SameLayoutHash_CacheHit()
        {
            // Given
            var pathCache = CreatePathCache();
            var start = new Vector2Int(0, 0);
            var goal = new Vector2Int(5, 5);
            
            pathCache.SetLayoutHash("hash_v1");
            var path = new CachedPath(
                new System.Collections.Generic.List<Vector2Int> { start, goal }, 
                10.0f, 
                true
            );
            pathCache.Put(start, goal, path);
            
            // When: 같은 layoutHash로 조회
            bool found = pathCache.TryGet(start, goal, out var result);
            
            // Then: 캐시 적중
            Assert.IsTrue(found, "같은 layoutHash로는 캐시를 찾을 수 있어야 함");
            Assert.AreEqual(10.0f, result.cost, "캐시된 비용이 일치해야 함");
        }

        [Test]
        [Description("AC-6.3: layoutHash 변경 시 이전 캐시 모두 무효화")]
        public void PathCache_LayoutHashChanged_InvalidatesAllCache()
        {
            // Given: 여러 경로를 캐시에 저장
            var pathCache = CreatePathCache();
            pathCache.SetLayoutHash("hash_v1");
            
            var paths = new[]
            {
                (new Vector2Int(0, 0), new Vector2Int(5, 5)),
                (new Vector2Int(1, 1), new Vector2Int(6, 6)),
                (new Vector2Int(2, 2), new Vector2Int(7, 7))
            };
            
            foreach (var (start, goal) in paths)
            {
                var path = new CachedPath(
                    new System.Collections.Generic.List<Vector2Int> { start, goal },
                    10.0f,
                    true
                );
                pathCache.Put(start, goal, path);
            }
            
            // 모두 캐시 적중 확인
            foreach (var (start, goal) in paths)
            {
                Assert.IsTrue(pathCache.TryGet(start, goal, out _), "저장한 경로를 찾을 수 있어야 함");
            }
            
            // When: layoutHash 변경
            pathCache.SetLayoutHash("hash_v2");
            
            // Then: 모든 경로가 무효화됨
            foreach (var (start, goal) in paths)
            {
                Assert.IsFalse(pathCache.TryGet(start, goal, out _), 
                    "layoutHash 변경 후 모든 캐시가 무효화되어야 함");
            }
        }

        [Test]
        [Description("layoutHash가 비어있어도 동작")]
        public void PathCache_EmptyLayoutHash_WorksCorrectly()
        {
            // Given
            var pathCache = CreatePathCache();
            pathCache.SetLayoutHash(""); // 빈 문자열
            
            var start = new Vector2Int(0, 0);
            var goal = new Vector2Int(5, 5);
            var path = new CachedPath(
                new System.Collections.Generic.List<Vector2Int> { start, goal },
                10.0f,
                true
            );
            
            // When
            pathCache.Put(start, goal, path);
            bool found = pathCache.TryGet(start, goal, out _);
            
            // Then
            Assert.IsTrue(found, "빈 layoutHash도 정상 동작해야 함");
        }

        #endregion

        #region Helper Methods

        private PathCache CreatePathCache()
        {
            var go = new GameObject("TestPathCache");
            var cache = go.AddComponent<PathCache>();
            return cache;
        }

        #endregion
    }
}