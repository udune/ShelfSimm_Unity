using NUnit.Framework;
using Core;

namespace Tests
{
    public class CellLockManagerTests
    {
        private CellLockManager _lockManager;

        [SetUp]
        public void SetUp()
        {
            _lockManager = new CellLockManager();
        }

        [Test]
        public void TryLock_SucceedsForUnlockedCell()
        {
            // Arrange
            string cellCode = "A1-01";

            // Act
            bool result = _lockManager.TryLock(cellCode);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void TryLock_FailsForAlreadyLockedCell()
        {
            // Arrange
            string cellCode = "A1-01";
            _lockManager.TryLock(cellCode);

            // Act
            bool result = _lockManager.TryLock(cellCode);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void Unlock_ReleasesLock()
        {
            // Arrange
            string cellCode = "A1-01";
            _lockManager.TryLock(cellCode);

            // Act
            _lockManager.Unlock(cellCode);

            // Assert
            Assert.IsFalse(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void Unlock_CanBeCalledMultipleTimes()
        {
            // Arrange
            string cellCode = "A1-01";
            _lockManager.TryLock(cellCode);
            _lockManager.Unlock(cellCode);

            // Act & Assert (no exception)
            _lockManager.Unlock(cellCode);
            Assert.IsFalse(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void TryLock_SucceedsAfterUnlock()
        {
            // Arrange
            string cellCode = "A1-01";
            _lockManager.TryLock(cellCode);
            _lockManager.Unlock(cellCode);

            // Act
            bool result = _lockManager.TryLock(cellCode);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void MultipleCells_CanBeLockedIndependently()
        {
            // Arrange
            string cell1 = "A1-01";
            string cell2 = "A1-02";
            string cell3 = "A1-03";

            // Act
            bool lock1 = _lockManager.TryLock(cell1);
            bool lock2 = _lockManager.TryLock(cell2);
            bool lock3 = _lockManager.TryLock(cell3);

            // Assert
            Assert.IsTrue(lock1);
            Assert.IsTrue(lock2);
            Assert.IsTrue(lock3);
            Assert.AreEqual(3, _lockManager.GetLockedCellCount());
        }

        [Test]
        public void UnlockAll_ReleasesAllLocks()
        {
            // Arrange
            _lockManager.TryLock("A1-01");
            _lockManager.TryLock("A1-02");
            _lockManager.TryLock("A1-03");

            // Act
            _lockManager.UnlockAll();

            // Assert
            Assert.AreEqual(0, _lockManager.GetLockedCellCount());
            Assert.IsFalse(_lockManager.IsLocked("A1-01"));
            Assert.IsFalse(_lockManager.IsLocked("A1-02"));
            Assert.IsFalse(_lockManager.IsLocked("A1-03"));
        }

        [Test]
        public void CellLock_AutomaticallyUnlocksWhenDisposed()
        {
            // Arrange
            string cellCode = "A1-01";

            // Act
            using (var cellLock = new CellLock(_lockManager, cellCode))
            {
                // Assert during lock
                Assert.IsTrue(cellLock.IsAcquired);
                Assert.IsTrue(_lockManager.IsLocked(cellCode));
            }

            // Assert after disposal
            Assert.IsFalse(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void CellLock_FailsToAcquireIfAlreadyLocked()
        {
            // Arrange
            string cellCode = "A1-01";
            _lockManager.TryLock(cellCode);

            // Act
            using (var cellLock = new CellLock(_lockManager, cellCode))
            {
                // Assert
                Assert.IsFalse(cellLock.IsAcquired);
            }

            // Cell should still be locked (original lock not released)
            Assert.IsTrue(_lockManager.IsLocked(cellCode));
        }

        [Test]
        public void TryLock_ReturnsFalseForNullOrEmptyCellCode()
        {
            // Act & Assert
            Assert.IsFalse(_lockManager.TryLock(null));
            Assert.IsFalse(_lockManager.TryLock(""));
        }

        [Test]
        public void GetLockedCells_ReturnsAllLockedCells()
        {
            // Arrange
            _lockManager.TryLock("A1-01");
            _lockManager.TryLock("A1-02");

            // Act
            var lockedCells = _lockManager.GetLockedCells();

            // Assert
            Assert.AreEqual(2, lockedCells.Count);
            Assert.Contains("A1-01", lockedCells);
            Assert.Contains("A1-02", lockedCells);
        }
    }
}
