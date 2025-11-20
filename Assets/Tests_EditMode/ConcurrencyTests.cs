using NUnit.Framework;
using Core;
using Data;
using System.Collections.Generic;

namespace Tests
{
    /// <summary>
    /// T-603: 동시성 테스트 (락 단위: cell_code)
    /// AC-15.1: 동일 프레임에 PUT과 PICK이 겹쳐도 결과 수량이 음수/초과가 되지 않음
    /// 락 메커니즘을 통한 동시성 제어 검증
    /// </summary>
    public class ConcurrencyTests
    {
        private WarehouseInventory _warehouse;
        private CellLockManager _lockManager;
        private TransactionManager _transactionManager;

        [SetUp]
        public void SetUp()
        {
            _warehouse = new WarehouseInventory();
            _lockManager = new CellLockManager();
            _transactionManager = new TransactionManager(_lockManager);
        }

        #region Same Cell Concurrent Access

        [Test]
        public void ConcurrentPUT_SameCell_SecondOperationFails()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Manually lock the cell to simulate ongoing transaction
            _lockManager.TryLock("A1-01");

            // Act - Try to PUT while cell is locked
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);
            Assert.AreEqual(20, _warehouse.GetStock("Test Book")); // Warehouse unchanged
            Assert.AreEqual(0, cell.CurrentStock); // Cell unchanged

            // Cleanup
            _lockManager.Unlock("A1-01");
        }

        [Test]
        public void ConcurrentPICK_SameCell_SecondOperationFails()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 10);

            // Manually lock the cell to simulate ongoing transaction
            _lockManager.TryLock("A1-01");

            // Act - Try to PICK while cell is locked
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);
            Assert.AreEqual(0, _warehouse.GetStock("Test Book")); // Warehouse unchanged
            Assert.AreEqual(10, cell.CurrentStock); // Cell unchanged

            // Cleanup
            _lockManager.Unlock("A1-01");
        }

        [Test]
        public void ConcurrentPUT_PICK_SameCell_SecondOperationFails()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Simulate ongoing PUT transaction
            _lockManager.TryLock("A1-01");

            // Act - Try to PICK while PUT is in progress
            bool pickSuccess = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(pickSuccess);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);

            // Cleanup
            _lockManager.Unlock("A1-01");
        }

        [Test]
        public void SequentialOperations_SameCell_AfterLockRelease_Succeed()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act 1: First PUT (should succeed)
            bool put1 = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Act 2: Second PUT after lock release (should succeed)
            bool put2 = _transactionManager.ExecutePut(_warehouse, cell, book, 3, out _);

            // Assert
            Assert.IsTrue(put1);
            Assert.IsTrue(put2);
            Assert.AreEqual(12, _warehouse.GetStock("Test Book")); // 20 - 5 - 3 = 12
            Assert.AreEqual(8, cell.CurrentStock); // 5 + 3 = 8
        }

        #endregion

        #region Different Cells Concurrent Access

        [Test]
        public void ConcurrentPUT_DifferentCells_BothSucceed()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell1 = new Cell("A1-01", 500, 300);
            var cell2 = new Cell("A1-02", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act - PUT to different cells (different locks)
            bool put1 = _transactionManager.ExecutePut(_warehouse, cell1, book, 5, out _);
            bool put2 = _transactionManager.ExecutePut(_warehouse, cell2, book, 3, out _);

            // Assert
            Assert.IsTrue(put1);
            Assert.IsTrue(put2);
            Assert.AreEqual(12, _warehouse.GetStock("Test Book")); // 20 - 5 - 3 = 12
            Assert.AreEqual(5, cell1.CurrentStock);
            Assert.AreEqual(3, cell2.CurrentStock);
        }

        [Test]
        public void ConcurrentPICK_DifferentCells_BothSucceed()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell1 = new Cell("A1-01", 500, 300);
            var cell2 = new Cell("A1-02", 500, 300);
            cell1.PutBook(book, 10);
            cell2.PutBook(book, 8);

            // Act - PICK from different cells (different locks)
            bool pick1 = _transactionManager.ExecutePick(_warehouse, cell1, "BOOK001", "Test Book", 5, out _);
            bool pick2 = _transactionManager.ExecutePick(_warehouse, cell2, "BOOK001", "Test Book", 3, out _);

            // Assert
            Assert.IsTrue(pick1);
            Assert.IsTrue(pick2);
            Assert.AreEqual(8, _warehouse.GetStock("Test Book")); // 0 + 5 + 3 = 8
            Assert.AreEqual(5, cell1.CurrentStock); // 10 - 5 = 5
            Assert.AreEqual(5, cell2.CurrentStock); // 8 - 3 = 5
        }

        [Test]
        public void ConcurrentMixedOperations_DifferentCells_AllSucceed()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell1 = new Cell("A1-01", 500, 300);
            var cell2 = new Cell("A1-02", 500, 300);
            var cell3 = new Cell("A1-03", 500, 300);
            _warehouse.AddStock("Test Book", 30);
            cell2.PutBook(book, 10);

            // Act - Mixed operations on different cells
            bool put1 = _transactionManager.ExecutePut(_warehouse, cell1, book, 5, out _);
            bool pick2 = _transactionManager.ExecutePick(_warehouse, cell2, "BOOK001", "Test Book", 3, out _);
            bool put3 = _transactionManager.ExecutePut(_warehouse, cell3, book, 4, out _);

            // Assert
            Assert.IsTrue(put1);
            Assert.IsTrue(pick2);
            Assert.IsTrue(put3);
            Assert.AreEqual(24, _warehouse.GetStock("Test Book")); // 30 - 5 + 3 - 4 = 24
            Assert.AreEqual(5, cell1.CurrentStock);
            Assert.AreEqual(7, cell2.CurrentStock); // 10 - 3 = 7
            Assert.AreEqual(4, cell3.CurrentStock);
        }

        #endregion

        #region Lock Release Verification

        [Test]
        public void LockRelease_AfterSuccessfulTransaction()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act - Execute transaction
            _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Assert - Lock should be released
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        [Test]
        public void LockRelease_AfterFailedTransaction()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            // No warehouse stock

            // Act - Execute transaction (will fail)
            _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Assert - Lock should still be released
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        [Test]
        public void LockRelease_AfterException()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 400); // Too tall
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act - Execute transaction (will fail due to height)
            _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Assert - Lock should be released
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        #endregion

        #region Atomicity Under Concurrent Access

        [Test]
        public void NoNegativeStock_WhenConcurrentPICKAttempts()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 5);

            // Simulate first PICK holding lock
            _lockManager.TryLock("A1-01");

            // Act - Try to PICK more than available (should fail due to lock)
            bool pick1 = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 10, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(pick1);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);
            Assert.AreEqual(5, cell.CurrentStock); // Stock unchanged (protected by lock)

            // Cleanup
            _lockManager.Unlock("A1-01");
        }

        [Test]
        public void NoOverCapacity_WhenConcurrentPUTAttempts()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 100, 250); // Thick book
            var cell = new Cell("A1-01", 500, 300); // Can hold 5 books max
            _warehouse.AddStock("Test Book", 20);
            cell.PutBook(book, 3);

            // Simulate first PUT holding lock
            _lockManager.TryLock("A1-01");

            // Act - Try to PUT more than capacity (should fail due to lock)
            bool put1 = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(put1);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);
            Assert.AreEqual(3, cell.CurrentStock); // Stock unchanged (protected by lock)

            // Cleanup
            _lockManager.Unlock("A1-01");
        }

        [Test]
        public void ConsistentState_AfterConcurrentOperationsOnDifferentCells()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cells = new List<Cell>();
            for (int i = 1; i <= 5; i++)
            {
                cells.Add(new Cell($"A1-0{i}", 500, 300));
            }
            _warehouse.AddStock("Test Book", 100);

            // Act - Simulate multiple concurrent operations on different cells
            int totalPut = 0;
            for (int i = 0; i < 5; i++)
            {
                int quantity = (i + 1) * 2; // 2, 4, 6, 8, 10
                bool success = _transactionManager.ExecutePut(_warehouse, cells[i], book, quantity, out _);
                if (success) totalPut += quantity;
            }

            // Assert - Total should match
            int expectedWarehouse = 100 - totalPut;
            Assert.AreEqual(expectedWarehouse, _warehouse.GetStock("Test Book"));
            Assert.AreEqual(30, totalPut); // 2 + 4 + 6 + 8 + 10 = 30
        }

        #endregion

        #region Multiple Lock Verification

        [Test]
        public void MultipleCells_CanBeLockedIndependently()
        {
            // Arrange
            var cells = new[] { "A1-01", "A1-02", "A1-03" };

            // Act - Lock multiple cells
            foreach (var cellCode in cells)
            {
                bool locked = _lockManager.TryLock(cellCode);
                Assert.IsTrue(locked);
            }

            // Assert
            Assert.AreEqual(3, _lockManager.GetLockedCellCount());
            foreach (var cellCode in cells)
            {
                Assert.IsTrue(_lockManager.IsLocked(cellCode));
            }

            // Cleanup
            _lockManager.UnlockAll();
        }

        [Test]
        public void LockManager_TracksAllActiveLocks()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell1 = new Cell("A1-01", 500, 300);
            var cell2 = new Cell("A1-02", 500, 300);

            // Manually lock cells
            _lockManager.TryLock("A1-01");
            _lockManager.TryLock("A1-02");

            // Act
            var lockedCells = _lockManager.GetLockedCells();

            // Assert
            Assert.AreEqual(2, lockedCells.Count);
            Assert.Contains("A1-01", lockedCells);
            Assert.Contains("A1-02", lockedCells);

            // Cleanup
            _lockManager.UnlockAll();
        }

        #endregion

        #region Stress Tests

        [Test]
        public void StressTest_MultipleSequentialOperations_SameCell()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 100);

            // Act - Perform 10 PUT operations sequentially
            int successCount = 0;
            for (int i = 0; i < 10; i++)
            {
                bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 2, out _);
                if (success) successCount++;
            }

            // Assert
            Assert.AreEqual(10, successCount); // All should succeed (sequential)
            Assert.AreEqual(20, cell.CurrentStock); // 2 * 10 = 20
            Assert.AreEqual(80, _warehouse.GetStock("Test Book")); // 100 - 20 = 80
        }

        [Test]
        public void StressTest_AlternatingPUT_PICK_SameCell()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 100);

            // Act - Alternating PUT and PICK
            for (int i = 0; i < 5; i++)
            {
                _transactionManager.ExecutePut(_warehouse, cell, book, 4, out _);
                _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 2, out _);
            }

            // Assert
            Assert.AreEqual(10, cell.CurrentStock); // (4 - 2) * 5 = 10
            Assert.AreEqual(90, _warehouse.GetStock("Test Book")); // 100 - 20 + 10 = 90
        }

        [Test]
        public void StressTest_ManyDifferentCells_NoLockConflict()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cells = new List<Cell>();
            for (int i = 1; i <= 20; i++)
            {
                cells.Add(new Cell($"A1-{i:D2}", 500, 300));
            }
            _warehouse.AddStock("Test Book", 200);

            // Act - PUT to all cells
            int successCount = 0;
            for (int i = 0; i < 20; i++)
            {
                bool success = _transactionManager.ExecutePut(_warehouse, cells[i], book, 5, out _);
                if (success) successCount++;
            }

            // Assert
            Assert.AreEqual(20, successCount); // All should succeed (different locks)
            Assert.AreEqual(100, _warehouse.GetStock("Test Book")); // 200 - (5 * 20) = 100
        }

        #endregion

        #region Edge Cases

        [Test]
        public void ConcurrentAccess_WhenCellBecomesEmpty()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 5);

            // Act 1: PICK all books
            bool pick1 = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out _);

            // Act 2: Try to PICK from now-empty cell (should fail with BOOK_MISMATCH)
            bool pick2 = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 1, out ErrorCode errorCode);

            // Assert
            Assert.IsTrue(pick1);
            Assert.IsFalse(pick2);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode); // Cell is now empty
            Assert.IsTrue(cell.IsEmpty);
        }

        [Test]
        public void NoDeadlock_WhenSameCellAccessedSequentially()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 50);

            // Act - Rapid sequential access (no deadlock should occur)
            for (int i = 0; i < 10; i++)
            {
                bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 1, out ErrorCode errorCode);
                Assert.IsTrue(success, $"Iteration {i} failed with {errorCode}");
                Assert.IsFalse(_lockManager.IsLocked("A1-01"), $"Lock not released after iteration {i}");
            }

            // Assert
            Assert.AreEqual(10, cell.CurrentStock);
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        #endregion
    }
}
