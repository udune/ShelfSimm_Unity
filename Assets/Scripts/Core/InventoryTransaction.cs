using System;
using Data;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 재고 트랜잭션의 기본 추상 클래스
    /// </summary>
    public abstract class InventoryTransaction
    {
        public string TransactionId { get; private set; }
        public string CellCode { get; protected set; }
        public string BookTitle { get; protected set; }
        public int Quantity { get; protected set; }
        public ErrorCode ErrorCode { get; protected set; }
        public bool IsCommitted { get; private set; }
        public bool IsRolledBack { get; private set; }

        protected InventoryTransaction()
        {
            TransactionId = Guid.NewGuid().ToString();
            ErrorCode = ErrorCode.NONE;
            IsCommitted = false;
            IsRolledBack = false;
        }

        /// <summary>
        /// 트랜잭션을 실행합니다 (검증 + 변경 적용)
        /// </summary>
        public abstract bool Execute();

        /// <summary>
        /// 트랜잭션을 롤백합니다 (변경 사항 되돌림)
        /// </summary>
        public abstract void Rollback();

        /// <summary>
        /// 트랜잭션을 커밋합니다 (최종 확정)
        /// </summary>
        public void Commit()
        {
            if (IsRolledBack)
            {
                Debug.LogError($"[Transaction] Cannot commit a rolled back transaction: {TransactionId}");
                return;
            }

            IsCommitted = true;
            Debug.Log($"[Transaction] Committed: {TransactionId}");
        }

        /// <summary>
        /// 트랜잭션을 롤백으로 표시합니다
        /// </summary>
        protected void MarkAsRolledBack()
        {
            IsRolledBack = true;
            Debug.Log($"[Transaction] Rolled back: {TransactionId}");
        }
    }

    /// <summary>
    /// PUT 작업을 위한 트랜잭션
    /// 창고 재고 감소 (-) → 칸 재고 증가 (+)
    /// </summary>
    public class PutTransaction : InventoryTransaction
    {
        private readonly WarehouseInventory _warehouse;
        private readonly Cell _cell;
        private readonly Book _book;
        private bool _warehouseUpdated = false;
        private bool _cellUpdated = false;

        public PutTransaction(WarehouseInventory warehouse, Cell cell, Book book, int quantity)
        {
            _warehouse = warehouse;
            _cell = cell;
            _book = book;
            CellCode = cell.CellCode;
            BookTitle = book.Title;
            Quantity = quantity;
        }

        public override bool Execute()
        {
            // 1. 창고 재고 검증
            if (!_warehouse.CanRemoveStock(_book.Title, Quantity, out ErrorCode warehouseError))
            {
                ErrorCode = warehouseError;
                Debug.LogWarning($"[PutTransaction] Warehouse check failed: {ErrorCode.ToMessage()}");
                return false;
            }

            // 2. 칸 입고 검증
            if (!_cell.CanPutBook(_book, Quantity, out ErrorCode cellError))
            {
                ErrorCode = cellError;
                Debug.LogWarning($"[PutTransaction] Cell check failed: {ErrorCode.ToMessage()}");
                return false;
            }

            // 3. 창고 재고 감소
            try
            {
                _warehouse.RemoveStock(_book.Title, Quantity);
                _warehouseUpdated = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PutTransaction] Failed to remove warehouse stock: {ex.Message}");
                ErrorCode = ErrorCode.TRANSACTION_FAILED;
                Rollback();
                return false;
            }

            // 4. 칸 재고 증가
            try
            {
                _cell.PutBook(_book, Quantity);
                _cellUpdated = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PutTransaction] Failed to put book in cell: {ex.Message}");
                ErrorCode = ErrorCode.TRANSACTION_FAILED;
                Rollback();
                return false;
            }

            Debug.Log($"[PutTransaction] Success: {_book.Title} x{Quantity} → {CellCode}");
            return true;
        }

        public override void Rollback()
        {
            if (IsRolledBack)
                return;

            // 역순으로 롤백 (LIFO)
            if (_cellUpdated)
            {
                _cell.PickBook(Quantity); // 칸에서 제거
                Debug.Log($"[PutTransaction] Rollback: Removed {Quantity} from cell {CellCode}");
            }

            if (_warehouseUpdated)
            {
                _warehouse.AddStock(_book.Title, Quantity); // 창고로 복원
                Debug.Log($"[PutTransaction] Rollback: Restored {Quantity} to warehouse");
            }

            MarkAsRolledBack();
        }
    }

    /// <summary>
    /// PICK 작업을 위한 트랜잭션
    /// 칸 재고 감소 (-) → 창고 재고 증가 (+)
    /// </summary>
    public class PickTransaction : InventoryTransaction
    {
        private readonly WarehouseInventory _warehouse;
        private readonly Cell _cell;
        private readonly string _expectedBookId;
        private readonly string _expectedBookTitle;
        private bool _cellUpdated = false;
        private bool _warehouseUpdated = false;

        public PickTransaction(WarehouseInventory warehouse, Cell cell, string expectedBookId, string expectedBookTitle, int quantity)
        {
            _warehouse = warehouse;
            _cell = cell;
            _expectedBookId = expectedBookId;
            _expectedBookTitle = expectedBookTitle;
            CellCode = cell.CellCode;
            BookTitle = expectedBookTitle;
            Quantity = quantity;
        }

        // Backward compatibility constructor (uses title as ID)
        public PickTransaction(WarehouseInventory warehouse, Cell cell, string expectedBookTitle, int quantity)
            : this(warehouse, cell, expectedBookTitle, expectedBookTitle, quantity)
        {
        }

        public override bool Execute()
        {
            // 1. 도서 일치 검증 (BOOK_MISMATCH 우선 처리) - AC-15.2
            // 1.1. 빈 칸 검증 (book_id 누락 우선 처리)
            if (_cell.IsEmpty)
            {
                ErrorCode = ErrorCode.BOOK_MISMATCH;
                Debug.LogWarning($"[PickTransaction] Cell is empty: Expected book_id '{_expectedBookId}', but cell has no book");
                return false;
            }

            // 1.2. book_id 불일치 검증
            if (_cell.StoredBookId != _expectedBookId)
            {
                ErrorCode = ErrorCode.BOOK_MISMATCH;
                Debug.LogWarning($"[PickTransaction] Book ID mismatch: Expected '{_expectedBookId}', Found '{_cell.StoredBookId}'");
                return false;
            }

            // 2. 칸 재고 검증
            if (!_cell.CanPickBook(Quantity, out ErrorCode cellError))
            {
                ErrorCode = cellError;
                Debug.LogWarning($"[PickTransaction] Cell check failed: {ErrorCode.ToMessage()}");
                return false;
            }

            // 3. 칸 재고 감소
            try
            {
                _cell.PickBook(Quantity);
                _cellUpdated = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PickTransaction] Failed to pick book from cell: {ex.Message}");
                ErrorCode = ErrorCode.TRANSACTION_FAILED;
                Rollback();
                return false;
            }

            // 4. 창고 재고 증가
            try
            {
                _warehouse.AddStock(_expectedBookTitle, Quantity);
                _warehouseUpdated = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PickTransaction] Failed to add warehouse stock: {ex.Message}");
                ErrorCode = ErrorCode.TRANSACTION_FAILED;
                Rollback();
                return false;
            }

            Debug.Log($"[PickTransaction] Success: {_expectedBookTitle} x{Quantity} ← {CellCode}");
            return true;
        }

        public override void Rollback()
        {
            if (IsRolledBack)
                return;

            // 역순으로 롤백 (LIFO)
            if (_warehouseUpdated)
            {
                _warehouse.RemoveStock(_expectedBookTitle, Quantity); // 창고에서 제거
                Debug.Log($"[PickTransaction] Rollback: Removed {Quantity} from warehouse");
            }

            if (_cellUpdated)
            {
                // 칸에 다시 추가 - Book 객체 재구성 필요
                // 주의: 롤백 시 Book 객체 정보가 필요하므로 실제 사용 시 개선 필요
                Debug.LogWarning($"[PickTransaction] Rollback: Cell restoration requires Book object (not implemented in MVP)");
                // MVP에서는 단순히 로그만 남김
            }

            MarkAsRolledBack();
        }
    }
}
