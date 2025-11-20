using System.Collections.Generic;
using System.Linq; // ToList()를 사용하기 위해 추가
using UnityEngine;

namespace Core
{
    public class CellLockManager
    {
        private HashSet<string> _lockedCells = new HashSet<string>();

        public bool TryLock(string cellCode)
        {
            if (string.IsNullOrEmpty(cellCode))
            {
                return false;
            }

            // Add 메서드는 요소가 이미 존재하면 false를 반환합니다.
            // 이를 통해 Contains와 Add를 한 번에 처리할 수 있습니다.
            return _lockedCells.Add(cellCode);
        }

        public void Unlock(string cellCode)
        {
            if (!string.IsNullOrEmpty(cellCode))
            {
                _lockedCells.Remove(cellCode);
            }
        }

        public bool IsLocked(string cellCode)
        {
            return !string.IsNullOrEmpty(cellCode) && _lockedCells.Contains(cellCode);
        }

        public void UnlockAll()
        {
            _lockedCells.Clear();
        }

        public int GetLockedCellCount()
        {
            return _lockedCells.Count;
        }

        /// <summary>
        /// 잠긴 모든 셀 코드의 복사본을 리스트로 가져옵니다.
        /// </summary>
        public List<string> GetLockedCells()
        {
            // 내부의 HashSet을 수정하지 못하도록 복사본을 만들어 반환합니다.
            return new List<string>(_lockedCells);
        }
    }

    public class CellLock : System.IDisposable
    {
        private readonly CellLockManager _lockManager;
        private readonly string _cellCode;
        
        public bool IsAcquired { get; }

        public CellLock(CellLockManager lockManager, string cellCode)
        {
            _lockManager = lockManager;
            _cellCode = cellCode;
            IsAcquired = _lockManager.TryLock(cellCode);
        }

        public void Dispose()
        {
            if (IsAcquired)
            {
                _lockManager.Unlock(_cellCode);
            }
        }
    }
}
