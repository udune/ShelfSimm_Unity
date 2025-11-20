using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 셀 단위 잠금을 관리하는 클래스
    /// cell_code 단위로 락을 제공하여 동시성 제어를 수행합니다.
    /// </summary>
    public class CellLockManager
    {
        // 현재 잠긴 cell_code들을 추적
        private HashSet<string> _lockedCells = new HashSet<string>();

        /// <summary>
        /// 특정 셀을 잠급니다.
        /// </summary>
        /// <param name="cellCode">잠글 셀 코드</param>
        /// <returns>잠금 성공 여부 (이미 잠겨있으면 false)</returns>
        public bool TryLock(string cellCode)
        {
            if (string.IsNullOrEmpty(cellCode))
            {
                Debug.LogWarning("[CellLockManager] Invalid cell code");
                return false;
            }

            if (_lockedCells.Contains(cellCode))
            {
                Debug.LogWarning($"[CellLockManager] Cell already locked: {cellCode}");
                return false;
            }

            _lockedCells.Add(cellCode);
            Debug.Log($"[CellLockManager] Cell locked: {cellCode}");
            return true;
        }

        /// <summary>
        /// 특정 셀의 잠금을 해제합니다.
        /// </summary>
        /// <param name="cellCode">잠금 해제할 셀 코드</param>
        public void Unlock(string cellCode)
        {
            if (string.IsNullOrEmpty(cellCode))
            {
                Debug.LogWarning("[CellLockManager] Invalid cell code");
                return;
            }

            if (_lockedCells.Remove(cellCode))
            {
                Debug.Log($"[CellLockManager] Cell unlocked: {cellCode}");
            }
            else
            {
                Debug.LogWarning($"[CellLockManager] Attempted to unlock a cell that wasn't locked: {cellCode}");
            }
        }

        /// <summary>
        /// 특정 셀이 잠겨있는지 확인합니다.
        /// </summary>
        public bool IsLocked(string cellCode)
        {
            return _lockedCells.Contains(cellCode);
        }

        /// <summary>
        /// 모든 잠금을 해제합니다.
        /// </summary>
        public void UnlockAll()
        {
            int count = _lockedCells.Count;
            _lockedCells.Clear();
            Debug.Log($"[CellLockManager] All locks cleared ({count} cells)");
        }

        /// <summary>
        /// 현재 잠긴 셀의 개수를 가져옵니다.
        /// </summary>
        public int GetLockedCellCount()
        {
            return _lockedCells.Count;
        }

        /// <summary>
        /// 잠긴 모든 셀 코드를 가져옵니다. (디버깅 용도)
        /// </summary>
        public IReadOnlyCollection<string> GetLockedCells()
        {
            return _lockedCells;
        }
    }

    /// <summary>
    /// 셀 잠금을 자동으로 해제하는 헬퍼 클래스
    /// using 문과 함께 사용하여 자동 잠금 해제를 보장합니다.
    /// </summary>
    public class CellLock : System.IDisposable
    {
        private readonly CellLockManager _lockManager;
        private readonly string _cellCode;
        private readonly bool _acquired;

        public bool IsAcquired => _acquired;

        public CellLock(CellLockManager lockManager, string cellCode)
        {
            _lockManager = lockManager;
            _cellCode = cellCode;
            _acquired = lockManager.TryLock(cellCode);
        }

        public void Dispose()
        {
            if (_acquired)
            {
                _lockManager.Unlock(_cellCode);
            }
        }
    }
}
