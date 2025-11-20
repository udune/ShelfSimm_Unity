using NUnit.Framework;
using Core;
using Data;

namespace Tests
{
    public class InventoryTransactionTests
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

        #region PUT Transaction Tests

        [Test]
        public void PutTransaction_Success_WhenWarehouseHasStock()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 10);

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(ErrorCode.NONE, errorCode);
            Assert.AreEqual(5, _warehouse.GetStock("Test Book")); // 10 - 5 = 5
            Assert.AreEqual(5, cell.CurrentStock);
            Assert.AreEqual("Test Book", cell.StoredBookTitle);
        }

        [Test]
        public void PutTransaction_Fails_WhenWarehouseStockInsufficient()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 3);

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.WAREHOUSE_INSUFFICIENT_STOCK, errorCode);
            Assert.AreEqual(3, _warehouse.GetStock("Test Book")); // Unchanged
            Assert.AreEqual(0, cell.CurrentStock); // Unchanged
        }

        [Test]
        public void PutTransaction_Fails_WhenCellCapacityFull()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 100, 300); // Can hold 5 books max
            _warehouse.AddStock("Test Book", 10);

            // Fill cell to capacity
            cell.PutBook(book, 5);

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 1, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.CAPACITY_FULL, errorCode);
            Assert.AreEqual(10, _warehouse.GetStock("Test Book")); // Unchanged
            Assert.AreEqual(5, cell.CurrentStock); // Unchanged
        }

        [Test]
        public void PutTransaction_Fails_WhenBookTooTall()
        {
            // Arrange
            var book = new Book("Tall Book", 20, 400); // 400mm tall
            var cell = new Cell("A1-01", 500, 300); // Only 300mm high
            _warehouse.AddStock("Tall Book", 10);

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 1, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.HEIGHT_LIMIT, errorCode);
            Assert.AreEqual(10, _warehouse.GetStock("Tall Book")); // Unchanged
            Assert.AreEqual(0, cell.CurrentStock);
        }

        [Test]
        public void PutTransaction_Fails_WhenBookMismatch()
        {
            // Arrange
            var book1 = new Book("Book A", 20, 250);
            var book2 = new Book("Book B", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Book A", 10);
            _warehouse.AddStock("Book B", 10);

            // Put Book A first
            cell.PutBook(book1, 5);

            // Act - Try to put Book B (mismatch)
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book2, 1, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(10, _warehouse.GetStock("Book B")); // Unchanged
            Assert.AreEqual(5, cell.CurrentStock); // Unchanged (still Book A)
        }

        #endregion

        #region PICK Transaction Tests

        [Test]
        public void PickTransaction_Success_WhenCellHasStock()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 10);

            // Act
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(ErrorCode.NONE, errorCode);
            Assert.AreEqual(5, _warehouse.GetStock("Test Book")); // 0 + 5 = 5
            Assert.AreEqual(5, cell.CurrentStock); // 10 - 5 = 5
        }

        [Test]
        public void PickTransaction_Fails_WhenCellStockInsufficient()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 3);

            // Act
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.INSUFFICIENT_STOCK, errorCode);
            Assert.AreEqual(0, _warehouse.GetStock("Test Book")); // Unchanged
            Assert.AreEqual(3, cell.CurrentStock); // Unchanged
        }

        [Test]
        public void PickTransaction_Fails_WhenBookMismatch()
        {
            // Arrange
            var book = new Book("Book A", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 10);

            // Act - Try to pick "Book B" but cell has "Book A"
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "Book B", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(0, _warehouse.GetStock("Book B")); // Unchanged
            Assert.AreEqual(10, cell.CurrentStock); // Unchanged
        }

        [Test]
        public void PickTransaction_Fails_WhenCellIsEmpty()
        {
            // Arrange
            var cell = new Cell("A1-01", 500, 300);

            // Act
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(0, _warehouse.GetStock("Test Book"));
            Assert.AreEqual(0, cell.CurrentStock);
        }

        #endregion

        #region Cell Locking Tests

        [Test]
        public void Transaction_Fails_WhenCellIsLocked()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 10);
            _lockManager.TryLock("A1-01");

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.CELL_LOCKED, errorCode);
            Assert.AreEqual(10, _warehouse.GetStock("Test Book")); // Unchanged
            Assert.AreEqual(0, cell.CurrentStock);
        }

        [Test]
        public void Transaction_ReleasesLock_AfterCompletion()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 10);

            // Act
            _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Assert
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        [Test]
        public void Transaction_ReleasesLock_EvenAfterFailure()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            // No warehouse stock

            // Act
            _transactionManager.ExecutePut(_warehouse, cell, book, 5, out _);

            // Assert
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
        }

        #endregion

        #region Concurrency Tests

        [Test]
        public void MultipleCells_CanBeModifiedConcurrently()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell1 = new Cell("A1-01", 500, 300);
            var cell2 = new Cell("A1-02", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act
            bool success1 = _transactionManager.ExecutePut(_warehouse, cell1, book, 5, out _);
            bool success2 = _transactionManager.ExecutePut(_warehouse, cell2, book, 5, out _);

            // Assert
            Assert.IsTrue(success1);
            Assert.IsTrue(success2);
            Assert.AreEqual(10, _warehouse.GetStock("Test Book")); // 20 - 10 = 10
            Assert.AreEqual(5, cell1.CurrentStock);
            Assert.AreEqual(5, cell2.CurrentStock);
        }

        #endregion

        #region Atomicity Tests

        [Test]
        public void PutTransaction_IsAtomic_NoPartialChangesOnFailure()
        {
            // Arrange
            var book = new Book("Test Book", 20, 400); // Too tall
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 10);

            // Act
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 5, out ErrorCode errorCode);

            // Assert - Transaction failed completely
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.HEIGHT_LIMIT, errorCode);
            // Warehouse stock should NOT be deducted
            Assert.AreEqual(10, _warehouse.GetStock("Test Book"));
            // Cell should remain empty
            Assert.AreEqual(0, cell.CurrentStock);
        }

        [Test]
        public void PickTransaction_IsAtomic_NoPartialChangesOnFailure()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 3);

            // Act - Try to pick more than available
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "Test Book", 5, out ErrorCode errorCode);

            // Assert - Transaction failed completely
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.INSUFFICIENT_STOCK, errorCode);
            // Warehouse should NOT receive any stock
            Assert.AreEqual(0, _warehouse.GetStock("Test Book"));
            // Cell stock should remain unchanged
            Assert.AreEqual(3, cell.CurrentStock);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PutAndPick_WorkCorrectly_InSequence()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act 1: PUT 10 books
            bool putSuccess = _transactionManager.ExecutePut(_warehouse, cell, book, 10, out _);

            // Act 2: PICK 5 books
            bool pickSuccess = _transactionManager.ExecutePick(_warehouse, cell, "Test Book", 5, out _);

            // Assert
            Assert.IsTrue(putSuccess);
            Assert.IsTrue(pickSuccess);
            Assert.AreEqual(15, _warehouse.GetStock("Test Book")); // 20 - 10 + 5 = 15
            Assert.AreEqual(5, cell.CurrentStock); // 10 - 5 = 5
        }

        #endregion
    }
}
