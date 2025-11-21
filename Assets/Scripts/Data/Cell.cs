using System;
using UnityEngine;

namespace Data
{
    public class Cell
    {
        public string CellCode { get; private set; }
        public int WidthMm { get; private set; }
        public int HeightMm { get; private set; }
        public string StoredBookId { get; private set; }
        public string StoredBookTitle { get; private set; }
        public int CurrentStock { get; private set; }
        public int MaxCapacity { get; private set; }

        public bool IsEmpty => CurrentStock == 0;
        public bool IsFull => CurrentStock >= MaxCapacity;

        public Cell(string cellCode, int widthMm, int heightMm)
        {
            CellCode = cellCode;
            WidthMm = widthMm;
            HeightMm = heightMm;
            StoredBookId = null;
            StoredBookTitle = null;
            CurrentStock = 0;
            MaxCapacity = 0;
        }

        public bool CanPutBook(Book book, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            if (book.HeightMm > HeightMm)
            {
                errorCode = ErrorCode.HEIGHT_LIMIT;
                return false;
            }

            if (!IsEmpty && StoredBookId != book.BookId)
            {
                errorCode = ErrorCode.BOOK_MISMATCH;
                return false;
            }

            if (IsEmpty)
            {
                MaxCapacity = Mathf.FloorToInt((float)WidthMm / book.ThicknessMm);
                if (MaxCapacity == 0)
                {
                    errorCode = ErrorCode.CAPACITY_FULL;
                    return false;
                }
            }

            int remainingCapacity = MaxCapacity - CurrentStock;
            if (quantity > remainingCapacity)
            {
                errorCode = ErrorCode.CAPACITY_FULL;
                return false;
            }

            return true;
        }

        public void PutBook(Book book, int quantity)
        {
            if (IsEmpty)
            {
                StoredBookId = book.BookId;
                StoredBookTitle = book.Title;
                MaxCapacity = Mathf.FloorToInt((float)WidthMm / book.ThicknessMm);
            }
            CurrentStock += quantity;
        }

        public bool CanPickBook(int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            if (CurrentStock < quantity)
            {
                errorCode = ErrorCode.INSUFFICIENT_STOCK;
                return false;
            }
            return true;
        }

        public void PickBook(int quantity)
        {
            CurrentStock -= quantity;
            if (CurrentStock == 0)
            {
                StoredBookId = null;
                StoredBookTitle = null;
                MaxCapacity = 0;
            }
        }
    }
}
