using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 트랜잭션 생명주기를 관리하는 클래스
    /// 셀 잠금과 함께 트랜잭션의 실행, 커밋, 롤백을 조율합니다.
    /// </summary>
    public class TransactionManager
    {
        private readonly CellLockManager _lockManager;
        private readonly List<InventoryTransaction> _activeTransactions;

        public TransactionManager(CellLockManager lockManager)
        {
            _lockManager = lockManager;
            _activeTransactions = new List<InventoryTransaction>();
        }

        /// <summary>
        /// 트랜잭션을 실행합니다 (잠금 획득 → 실행 → 커밋/롤백 → 잠금 해제)
        /// </summary>
        /// <param name="transaction">실행할 트랜잭션</param>
        /// <returns>트랜잭션 성공 여부</returns>
        public bool ExecuteTransaction(InventoryTransaction transaction)
        {
            if (transaction == null)
            {
                Debug.LogError("[TransactionManager] Transaction is null");
                return false;
            }

            // 1. 셀 잠금 시도
            if (!_lockManager.TryLock(transaction.CellCode))
            {
                transaction.ErrorCode = ErrorCode.CELL_LOCKED;
                Debug.LogWarning($"[TransactionManager] Failed to acquire lock for cell: {transaction.CellCode}");
                return false;
            }

            // 2. 트랜잭션 실행
            bool success = false;
            try
            {
                _activeTransactions.Add(transaction);
                success = transaction.Execute();

                if (success)
                {
                    transaction.Commit();
                    Debug.Log($"[TransactionManager] Transaction committed: {transaction.TransactionId}");
                }
                else
                {
                    // Execute가 실패하면 자동으로 Rollback 호출됨 (InventoryTransaction 내부에서 처리)
                    Debug.LogWarning($"[TransactionManager] Transaction failed: {transaction.ErrorCode.ToMessage()}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TransactionManager] Unexpected error: {ex.Message}");
                transaction.Rollback();
                success = false;
            }
            finally
            {
                // 3. 셀 잠금 해제
                _lockManager.Unlock(transaction.CellCode);
                _activeTransactions.Remove(transaction);
            }

            return success;
        }

        /// <summary>
        /// PUT 작업을 트랜잭션으로 실행합니다.
        /// </summary>
        public bool ExecutePut(WarehouseInventory warehouse, Cell cell, Book book, int quantity, out ErrorCode errorCode)
        {
            var transaction = new PutTransaction(warehouse, cell, book, quantity);
            bool success = ExecuteTransaction(transaction);
            errorCode = transaction.ErrorCode;
            return success;
        }

        /// <summary>
        /// PICK 작업을 트랜잭션으로 실행합니다. (book_id + title)
        /// </summary>
        public bool ExecutePick(WarehouseInventory warehouse, Cell cell, string expectedBookId, string expectedBookTitle, int quantity, out ErrorCode errorCode)
        {
            var transaction = new PickTransaction(warehouse, cell, expectedBookId, expectedBookTitle, quantity);
            bool success = ExecuteTransaction(transaction);
            errorCode = transaction.ErrorCode;
            return success;
        }

        /// <summary>
        /// PICK 작업을 트랜잭션으로 실행합니다. (backward compatibility - title only)
        /// </summary>
        public bool ExecutePick(WarehouseInventory warehouse, Cell cell, string expectedBookTitle, int quantity, out ErrorCode errorCode)
        {
            var transaction = new PickTransaction(warehouse, cell, expectedBookTitle, quantity);
            bool success = ExecuteTransaction(transaction);
            errorCode = transaction.ErrorCode;
            return success;
        }

        /// <summary>
        /// 현재 활성 트랜잭션 개수를 가져옵니다.
        /// </summary>
        public int GetActiveTransactionCount()
        {
            return _activeTransactions.Count;
        }

        /// <summary>
        /// 모든 활성 트랜잭션을 롤백하고 잠금을 해제합니다. (긴급 상황 대응)
        /// </summary>
        public void RollbackAll()
        {
            Debug.LogWarning($"[TransactionManager] Rolling back all active transactions ({_activeTransactions.Count})");

            foreach (var transaction in _activeTransactions)
            {
                try
                {
                    transaction.Rollback();
                    _lockManager.Unlock(transaction.CellCode);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[TransactionManager] Error during rollback: {ex.Message}");
                }
            }

            _activeTransactions.Clear();
            _lockManager.UnlockAll();
        }

        /// <summary>
        /// 특정 셀에 대한 트랜잭션이 진행 중인지 확인합니다.
        /// </summary>
        public bool IsTransactionActiveForCell(string cellCode)
        {
            return _lockManager.IsLocked(cellCode);
        }
    }
}
