using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class WarehouseInventory
    {
        private Dictionary<string, int> _stockByBookTitle = new Dictionary<string, int>();

        public int GetStock(string bookTitle)
        {
            if (_stockByBookTitle.TryGetValue(bookTitle, out int stock))
            {
                return stock;
            }
            return 0;
        }

        public void AddStock(string bookTitle, int quantity)
        {
            if (quantity <= 0) return;

            if (_stockByBookTitle.ContainsKey(bookTitle))
            {
                _stockByBookTitle[bookTitle] += quantity;
            }
            else
            {
                _stockByBookTitle[bookTitle] = quantity;
            }
        }

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
                return false;
            }

            return true;
        }

        public void RemoveStock(string bookTitle, int quantity)
        {
            if (!_stockByBookTitle.ContainsKey(bookTitle)) return;

            _stockByBookTitle[bookTitle] -= quantity;

            if (_stockByBookTitle[bookTitle] == 0)
            {
                _stockByBookTitle.Remove(bookTitle);
            }
        }

        public void InitializeStock(Dictionary<string, int> initialStock)
        {
            _stockByBookTitle.Clear();
            foreach (var kvp in initialStock)
            {
                _stockByBookTitle[kvp.Key] = kvp.Value;
            }
        }

        public void Clear()
        {
            _stockByBookTitle.Clear();
        }

        public IReadOnlyDictionary<string, int> GetAllStock()
        {
            return _stockByBookTitle;
        }
    }
}
