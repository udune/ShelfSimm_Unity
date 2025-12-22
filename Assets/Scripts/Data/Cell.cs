using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    public class Cell
    {
        // 책 재고 정보를 저장하는 내부 클래스
        private class BookStock
        {
            public string Title { get; set; }
            public int Quantity { get; set; }
            public int Thickness { get; set; }
        }

        public string CellCode { get; private set; }
        public int WidthMm { get; private set; }
        public int HeightMm { get; private set; }

        // 여러 종류의 책을 저장 (bookId -> BookStock)
        private Dictionary<string, BookStock> books = new Dictionary<string, BookStock>();

        // 하위 호환성을 위한 속성들 (첫 번째 책 정보 반환)
        public string StoredBookId => books.Keys.FirstOrDefault();
        public string StoredBookTitle => books.Values.FirstOrDefault()?.Title;
        public int CurrentStock => books.Values.Sum(b => b.Quantity);
        public int MaxCapacity => Mathf.FloorToInt((float)WidthMm / GetMinThickness());

        public bool IsEmpty => books.Count == 0;
        public bool IsFull => GetUsedWidth() >= WidthMm;

        public Cell(string cellCode, int widthMm, int heightMm)
        {
            CellCode = cellCode;
            WidthMm = widthMm;
            HeightMm = heightMm;
        }

        // 사용 중인 너비 계산
        private int GetUsedWidth()
        {
            return books.Values.Sum(b => b.Thickness * b.Quantity);
        }

        // 최소 두께 반환 (MaxCapacity 계산용)
        private int GetMinThickness()
        {
            if (books.Count == 0) return 1;
            return books.Values.Min(b => b.Thickness);
        }

        // 남은 너비 계산
        private int GetRemainingWidth()
        {
            return WidthMm - GetUsedWidth();
        }

        public bool CanPutBook(BookData book, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            if (book.Height > HeightMm)
            {
                errorCode = ErrorCode.HEIGHT_LIMIT;
                return false;
            }

            // 현재 저장된 책이 있는 경우, 추가로 필요한 너비 계산
            int requiredWidth;
            if (books.ContainsKey(book.Id))
            {
                // 이미 저장된 책이면 추가 수량만큼의 너비만 필요
                requiredWidth = book.Thickness * quantity;
            }
            else
            {
                // 새로운 책이면 전체 수량만큼의 너비 필요
                requiredWidth = book.Thickness * quantity;
            }

            int remainingWidth = GetRemainingWidth();
            if (requiredWidth > remainingWidth)
            {
                errorCode = ErrorCode.CAPACITY_FULL;
                return false;
            }

            return true;
        }

        public void PutBook(BookData book, int quantity)
        {
            if (books.ContainsKey(book.Id))
            {
                // 이미 저장된 책이면 수량만 증가
                books[book.Id].Quantity += quantity;
            }
            else
            {
                // 새로운 책이면 추가
                books[book.Id] = new BookStock
                {
                    Title = book.Title,
                    Quantity = quantity,
                    Thickness = book.Thickness
                };
            }
        }

        public bool CanPickBook(BookData book, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                return false;
            }

            if (!books.ContainsKey(book.Id))
            {
                errorCode = ErrorCode.BOOK_MISMATCH;
                return false;
            }

            if (books[book.Id].Quantity < quantity)
            {
                errorCode = ErrorCode.INSUFFICIENT_STOCK;
                return false;
            }

            return true;
        }

        public void PickBook(BookData book, int quantity)
        {
            if (!books.ContainsKey(book.Id))
            {
                Debug.LogWarning($"Cell {CellCode}에 책 {book.Title}이 없습니다.");
                return;
            }

            books[book.Id].Quantity -= quantity;

            // 재고가 0이 되면 제거
            if (books[book.Id].Quantity <= 0)
            {
                books.Remove(book.Id);
            }
        }

        // Cell에 저장된 모든 책 정보 반환 (디버깅/UI용)
        public List<(string bookId, string title, int quantity)> GetAllBooks()
        {
            return books.Select(kvp => (kvp.Key, kvp.Value.Title, kvp.Value.Quantity)).ToList();
        }

        // 특정 책의 재고 조회
        public int GetBookQuantity(string bookId)
        {
            return books.ContainsKey(bookId) ? books[bookId].Quantity : 0;
        }
    }
}