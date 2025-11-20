using NUnit.Framework;
using Core;
using Data;

namespace Tests
{
    /// <summary>
    /// T-602: BOOK_MISMATCH 검증 (book_id 누락 우선 처리)
    /// AC-15.2: PICK 도서 ID가 칸의 book_id와 다르면 result=FAIL, fail_reason=BOOK_MISMATCH
    /// </summary>
    public class BookIdValidationTests
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

        #region Book ID Priority Tests

        [Test]
        public void PickTransaction_Fails_WhenCellIsEmpty_BookIdNull()
        {
            // Arrange
            var cell = new Cell("A1-01", 500, 300);

            // Act - Try to PICK from empty cell
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(0, cell.CurrentStock); // Cell remains empty
        }

        [Test]
        public void PickTransaction_Fails_WhenBookIdMismatch()
        {
            // Arrange
            var book1 = new Book("BOOK001", "Book A", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book1, 10);

            // Act - Try to PICK different book_id
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK002", "Book B", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(10, cell.CurrentStock); // Cell unchanged
            Assert.AreEqual("BOOK001", cell.StoredBookId);
        }

        [Test]
        public void PickTransaction_Success_WhenBookIdMatches()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 10);

            // Act - PICK with matching book_id
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out ErrorCode errorCode);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(ErrorCode.NONE, errorCode);
            Assert.AreEqual(5, cell.CurrentStock); // 10 - 5 = 5
            Assert.AreEqual("BOOK001", cell.StoredBookId);
            Assert.AreEqual(5, _warehouse.GetStock("Test Book"));
        }

        [Test]
        public void PutTransaction_Fails_WhenBookIdMismatch()
        {
            // Arrange
            var book1 = new Book("BOOK001", "Book A", 20, 250);
            var book2 = new Book("BOOK002", "Book B", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Book A", 10);
            _warehouse.AddStock("Book B", 10);

            // Put Book A first
            cell.PutBook(book1, 5);

            // Act - Try to PUT Book B (different book_id)
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book2, 3, out ErrorCode errorCode);

            // Assert
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
            Assert.AreEqual(10, _warehouse.GetStock("Book B")); // Warehouse unchanged
            Assert.AreEqual(5, cell.CurrentStock); // Cell unchanged (still Book A)
            Assert.AreEqual("BOOK001", cell.StoredBookId);
        }

        [Test]
        public void PutTransaction_Success_WhenBookIdMatches()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Put first batch
            cell.PutBook(book, 5);

            // Act - PUT more of same book_id
            bool success = _transactionManager.ExecutePut(_warehouse, cell, book, 3, out ErrorCode errorCode);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(ErrorCode.NONE, errorCode);
            Assert.AreEqual(17, _warehouse.GetStock("Test Book")); // 20 - 3 = 17
            Assert.AreEqual(8, cell.CurrentStock); // 5 + 3 = 8
            Assert.AreEqual("BOOK001", cell.StoredBookId);
        }

        #endregion

        #region Cell State Management

        [Test]
        public void Cell_ClearsBookId_WhenEmptied()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 5);

            // Act - Pick all books
            cell.PickBook(5);

            // Assert
            Assert.IsTrue(cell.IsEmpty);
            Assert.IsNull(cell.StoredBookId);
            Assert.IsNull(cell.StoredBookTitle);
            Assert.AreEqual(0, cell.MaxCapacity);
        }

        [Test]
        public void Cell_RetainsBookId_WhenPartiallyEmptied()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 10);

            // Act - Pick some books
            cell.PickBook(3);

            // Assert
            Assert.IsFalse(cell.IsEmpty);
            Assert.AreEqual("BOOK001", cell.StoredBookId);
            Assert.AreEqual("Test Book", cell.StoredBookTitle);
            Assert.AreEqual(7, cell.CurrentStock);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullCycle_PutAndPick_WithBookId()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            _warehouse.AddStock("Test Book", 20);

            // Act 1: PUT 10 books
            bool putSuccess = _transactionManager.ExecutePut(_warehouse, cell, book, 10, out _);

            // Act 2: PICK 5 books with book_id
            bool pickSuccess = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out _);

            // Assert
            Assert.IsTrue(putSuccess);
            Assert.IsTrue(pickSuccess);
            Assert.AreEqual(15, _warehouse.GetStock("Test Book")); // 20 - 10 + 5 = 15
            Assert.AreEqual(5, cell.CurrentStock); // 10 - 5 = 5
            Assert.AreEqual("BOOK001", cell.StoredBookId);
        }

        [Test]
        public void BackwardCompatibility_OldConstructor_UsesTitle()
        {
            // Arrange
            var book = new Book("Test Book", 20, 250); // Old constructor
            var cell = new Cell("A1-01", 500, 300);

            // Act
            cell.PutBook(book, 5);

            // Assert
            Assert.AreEqual("Test Book", book.BookId); // Title used as ID
            Assert.AreEqual("Test Book", cell.StoredBookId);
            Assert.AreEqual("Test Book", cell.StoredBookTitle);
        }

        #endregion

        #region Error Priority Tests

        [Test]
        public void PickTransaction_BookIdMismatch_TakesPriority_OverStockCheck()
        {
            // Arrange
            var book = new Book("BOOK001", "Test Book", 20, 250);
            var cell = new Cell("A1-01", 500, 300);
            cell.PutBook(book, 3); // Only 3 books

            // Act - Try to PICK 5 books with wrong book_id (both errors present)
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK002", "Wrong Book", 5, out ErrorCode errorCode);

            // Assert - BOOK_MISMATCH should be reported, not INSUFFICIENT_STOCK
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
        }

        [Test]
        public void PickTransaction_EmptyCell_TakesPriority_OverQuantityCheck()
        {
            // Arrange
            var cell = new Cell("A1-01", 500, 300); // Empty cell

            // Act - Try to PICK from empty cell
            bool success = _transactionManager.ExecutePick(_warehouse, cell, "BOOK001", "Test Book", 5, out ErrorCode errorCode);

            // Assert - BOOK_MISMATCH (empty cell) should be reported first
            Assert.IsFalse(success);
            Assert.AreEqual(ErrorCode.BOOK_MISMATCH, errorCode);
        }

        #endregion
    }
}
