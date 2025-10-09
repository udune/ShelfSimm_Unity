using NUnit.Framework;
using UnityEngine;
using Core;
using Managers;
using Data;

namespace Tests.EditMode
{
    /// <summary>
    /// T-205: 캐시 키에 layout_hash 포함 + 전역 퍼지 테스트
    /// AC-6.3 검증
    /// </summary>
    [TestFixture]
    public class LayoutHashManagerTests
    {
        private GameObject testObject;
        private LayoutHashManager layoutHashManager;
        private PathCache pathCache;
        private CellsLayoutSO testLayout;

        [SetUp]
        public void Setup()
        {
            // 테스트용 게임 오브젝트 생성
            testObject = new GameObject("TestLayoutHashManager");
            
            // PathCache 컴포넌트 생성
            pathCache = testObject.AddComponent<PathCache>();
            
            // LayoutHashManager 컴포넌트 생성 및 연결
            layoutHashManager = testObject.AddComponent<LayoutHashManager>();
            var field = typeof(LayoutHashManager).GetField("pathCache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(layoutHashManager, pathCache);
            
            // 테스트용 레이아웃 생성
            testLayout = ScriptableObject.CreateInstance<CellsLayoutSO>();
            testLayout.schema_version = "1.0";
            testLayout.grid_size = new Vector2Int(10, 10);
            testLayout.warehouse = new Vector2Int(0, 0);
            testLayout.cells = new System.Collections.Generic.List<CellDef>
            {
                new CellDef("D20", 1, 2, 90, 200),
                new CellDef("A15", 5, 3, 90, 200)
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            
            if (testLayout != null)
            {
                Object.DestroyImmediate(testLayout);
            }
        }

        #region AC-6.3: 레이아웃 변경 시 캐시 무효화

        [Test]
        [Description("AC-6.3: 레이아웃 해시가 계산되어 설정됨")]
        public void UpdateLayoutHash_ValidLayout_ComputesHash()
        {
            // When: 레이아웃 해시 업데이트
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then: 해시가 계산되어 설정됨
            Assert.IsNotEmpty(testLayout.layout_hash, "layout_hash가 비어있지 않아야 함");
            Assert.IsTrue(testLayout.layout_hash.StartsWith("sha256:"), "해시는 'sha256:' 접두사를 가져야 함");
            
            Debug.Log($"계산된 해시: {testLayout.layout_hash}");
        }

        [Test]
        [Description("AC-6.3: 레이아웃 변경 시 해시가 달라짐")]
        public void UpdateLayoutHash_LayoutChanged_HashChanges()
        {
            // Given: 초기 해시 계산
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 레이아웃 변경 (셀 추가)
            testLayout.cells.Add(new CellDef("B03", 3, 1, 90, 200));
            layoutHashManager.UpdateLayoutHash(testLayout);
            string newHash = testLayout.layout_hash;
            
            // Then: 해시가 변경됨
            Assert.AreNotEqual(initialHash, newHash, "레이아웃 변경 시 해시가 달라져야 함");
            
            Debug.Log($"이전 해시: {initialHash}");
            Debug.Log($"새 해시: {newHash}");
        }

        [Test]
        [Description("AC-6.3: 레이아웃 변경 시 PathCache 전역 무효화")]
        public void UpdateLayoutHash_LayoutChanged_InvalidatesPathCache()
        {
            // Given: 초기 해시 설정 및 캐시 데이터 추가
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // 캐시에 경로 추가
            var start = new Vector2Int(0, 0);
            var goal = new Vector2Int(5, 5);
            var cachedPath = new CachedPath(
                new System.Collections.Generic.List<Vector2Int> { start, goal }, 
                10.0f, 
                true
            );
            pathCache.Put(start, goal, cachedPath);
            
            // 캐시 적중 확인
            bool hitBefore = pathCache.TryGet(start, goal, out _);
            Assert.IsTrue(hitBefore, "캐시에서 경로를 찾을 수 있어야 함");
            
            // When: 레이아웃 변경
            testLayout.cells[0].x = 10; // D20의 위치 변경
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then: 캐시가 무효화됨 (같은 키로 조회해도 미스)
            bool hitAfter = pathCache.TryGet(start, goal, out _);
            Assert.IsFalse(hitAfter, "레이아웃 변경 후 캐시가 무효화되어야 함");
        }

        [Test]
        [Description("AC-6.3: 동일한 레이아웃은 동일한 해시 생성")]
        public void UpdateLayoutHash_SameLayout_ProducesSameHash()
        {
            // Given: 동일한 레이아웃으로 두 번 계산
            layoutHashManager.UpdateLayoutHash(testLayout);
            string firstHash = testLayout.layout_hash;
            
            // When: 다시 계산
            layoutHashManager.UpdateLayoutHash(testLayout);
            string secondHash = testLayout.layout_hash;
            
            // Then: 동일한 해시 생성
            Assert.AreEqual(firstHash, secondHash, "동일한 레이아웃은 동일한 해시를 생성해야 함");
        }

        [Test]
        [Description("AC-6.3: 셀 순서가 달라도 동일한 해시 (정렬 보장)")]
        public void UpdateLayoutHash_DifferentOrder_ProducesSameHash()
        {
            // Given: 첫 번째 레이아웃
            layoutHashManager.UpdateLayoutHash(testLayout);
            string firstHash = testLayout.layout_hash;
            
            // When: 셀 순서를 바꾼 두 번째 레이아웃
            var layout2 = ScriptableObject.CreateInstance<CellsLayoutSO>();
            layout2.schema_version = testLayout.schema_version;
            layout2.grid_size = testLayout.grid_size;
            layout2.warehouse = testLayout.warehouse;
            layout2.cells = new System.Collections.Generic.List<CellDef>
            {
                testLayout.cells[1], // A15를 먼저
                testLayout.cells[0]  // D20을 나중에
            };
            
            layoutHashManager.UpdateLayoutHash(layout2);
            string secondHash = layout2.layout_hash;
            
            // Then: 동일한 해시 (내부에서 정렬함)
            Assert.AreEqual(firstHash, secondHash, "셀 순서가 달라도 동일한 해시를 생성해야 함");
            
            Object.DestroyImmediate(layout2);
        }

        [Test]
        [Description("AC-6.3: null 레이아웃은 처리하지 않음")]
        public void UpdateLayoutHash_NullLayout_DoesNotThrow()
        {
            // When & Then: null 레이아웃 전달 시 예외가 발생하지 않아야 함
            Assert.DoesNotThrow(() => layoutHashManager.UpdateLayoutHash(null));
        }

        #endregion

        #region 해시 계산 상세 테스트

        [Test]
        [Description("셀 위치 변경 시 해시 변경")]
        public void UpdateLayoutHash_CellPositionChanged_HashChanges()
        {
            // Given
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 셀 위치 변경
            testLayout.cells[0].x = 99;
            testLayout.cells[0].y = 99;
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then
            Assert.AreNotEqual(initialHash, testLayout.layout_hash);
        }

        [Test]
        [Description("셀 크기 변경 시 해시 변경")]
        public void UpdateLayoutHash_CellSizeChanged_HashChanges()
        {
            // Given
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 셀 크기 변경
            testLayout.cells[0].width = 100;
            testLayout.cells[0].height = 250;
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then
            Assert.AreNotEqual(initialHash, testLayout.layout_hash);
        }

        [Test]
        [Description("셀 차단 상태 변경 시 해시 변경")]
        public void UpdateLayoutHash_CellBlockedChanged_HashChanges()
        {
            // Given
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 차단 상태 변경
            testLayout.cells[0].blocked = true;
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then
            Assert.AreNotEqual(initialHash, testLayout.layout_hash);
        }

        [Test]
        [Description("격자 크기 변경 시 해시 변경")]
        public void UpdateLayoutHash_GridSizeChanged_HashChanges()
        {
            // Given
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 격자 크기 변경
            testLayout.grid_size = new Vector2Int(20, 20);
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then
            Assert.AreNotEqual(initialHash, testLayout.layout_hash);
        }

        [Test]
        [Description("창고 위치 변경 시 해시 변경")]
        public void UpdateLayoutHash_WarehouseChanged_HashChanges()
        {
            // Given
            layoutHashManager.UpdateLayoutHash(testLayout);
            string initialHash = testLayout.layout_hash;
            
            // When: 창고 위치 변경
            testLayout.warehouse = new Vector2Int(5, 5);
            layoutHashManager.UpdateLayoutHash(testLayout);
            
            // Then
            Assert.AreNotEqual(initialHash, testLayout.layout_hash);
        }

        #endregion

        #region GetLastComputedHash 테스트

        [Test]
        [Description("마지막 계산된 해시를 반환")]
        public void GetLastComputedHash_AfterUpdate_ReturnsComputedHash()
        {
            // When
            layoutHashManager.UpdateLayoutHash(testLayout);
            string lastHash = layoutHashManager.GetLastComputedHash();
            
            // Then
            Assert.IsNotEmpty(lastHash, "마지막 계산된 해시가 비어있지 않아야 함");
            Assert.AreEqual(testLayout.layout_hash, lastHash, "레이아웃의 해시와 일치해야 함");
        }

        [Test]
        [Description("업데이트 전에는 빈 문자열 반환")]
        public void GetLastComputedHash_BeforeUpdate_ReturnsEmpty()
        {
            // When
            string lastHash = layoutHashManager.GetLastComputedHash();
            
            // Then
            Assert.IsEmpty(lastHash, "업데이트 전에는 빈 문자열이어야 함");
        }

        #endregion
    }
}