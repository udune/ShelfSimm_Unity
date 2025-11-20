using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// 창고의 재고를 관리하는 클래스
    /// 도서별 재고 수량을 추적하고 입출고를 처리합니다.
    /// </summary>
    public class WarehouseInventory
    {
        // 도서 제목별 재고 수량
        private Dictionary<string, int> _stockByBookTitle = new Dictionary<string, int>();

        /// <summary>
        /// 특정 도서의 현재 재고를 가져옵니다.
        /// </summary>
        public int GetStock(string bookTitle)
        {
            if (_stockByBookTitle.TryGetValue(bookTitle, out int stock))
            {
                return stock;
            }
            return 0;
        }

        /// <summary>
        /// 창고에 도서를 입고합니다.
        /// </summary>
        public void AddStock(string bookTitle, int quantity)
        {
            if (quantity <= 0)
            {
                Debug.LogWarning($"[WarehouseInventory] Invalid quantity: {quantity}");
                return;
            }

            if (_stockByBookTitle.ContainsKey(bookTitle))
            {
                _stockByBookTitle[bookTitle] += quantity;
            }
            else
            {
                _stockByBookTitle[bookTitle] = quantity;
            }

            Debug.Log($"[WarehouseInventory] {bookTitle} {quantity}권 입고. 현재 재고: {_stockByBookTitle[bookTitle]}권");
        }

        /// <summary>
        /// 창고에서 도서를 출고할 수 있는지 확인합니다.
        /// </summary>
        public bool CanRemoveStock(string bookTitle, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            int currentStock = GetStock(bookTitle);
            if (currentStock < quantity)
            {
                errorCode = ErrorCode.WAREHOUSE_INSUFFICIENT_STOCK;
                Debug.LogWarning($"[WarehouseInventory] 재고 부족: {bookTitle} (현재: {currentStock}권, 요청: {quantity}권)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 창고에서 도서를 출고합니다. (CanRemoveStock 검증 후 호출)
        /// </summary>
        public void RemoveStock(string bookTitle, int quantity)
        {
            if (!_stockByBookTitle.ContainsKey(bookTitle))
            {
                Debug.LogError($"[WarehouseInventory] 존재하지 않는 도서: {bookTitle}");
                return;
            }

            _stockByBookTitle[bookTitle] -= quantity;

            if (_stockByBookTitle[bookTitle] == 0)
            {
                _stockByBookTitle.Remove(bookTitle);
            }

            Debug.Log($"[WarehouseInventory] {bookTitle} {quantity}권 출고. 남은 재고: {GetStock(bookTitle)}권");
        }

        /// <summary>
        /// 초기 재고를 설정합니다. (시뮬레이션 시작 시 호출)
        /// </summary>
        public void InitializeStock(Dictionary<string, int> initialStock)
        {
            _stockByBookTitle.Clear();
            foreach (var kvp in initialStock)
            {
                _stockByBookTitle[kvp.Key] = kvp.Value;
            }
            Debug.Log($"[WarehouseInventory] 초기 재고 설정 완료: {_stockByBookTitle.Count}종 도서");
        }

        /// <summary>
        /// 모든 재고를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            _stockByBookTitle.Clear();
        }

        /// <summary>
        /// 전체 재고 목록을 가져옵니다. (읽기 전용)
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllStock()
        {
            return _stockByBookTitle;
        }
    }
}
