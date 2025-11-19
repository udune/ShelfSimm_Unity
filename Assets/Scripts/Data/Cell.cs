using System;
using UnityEngine;
using Core.Core;

namespace Data
{
    public class Cell
    {
        public string CellCode { get; private set; }
        public int WidthMm { get; private set; }
        public int HeightMm { get; private set; }
        public string StoredBookTitle { get; private set; } // 현재 보관된 책의 제목
        public int CurrentStock { get; private set; } // 현재 재고 수량

        public int MaxCapacity { get; private set; } // 최대 보관 가능 수량 (책 두께에 따라 달라짐)

        public bool IsEmpty => CurrentStock == 0;
        public bool IsFull => CurrentStock >= MaxCapacity;

        public Cell(string cellCode, int widthMm, int heightMm)
        {
            CellCode = cellCode;
            WidthMm = widthMm;
            HeightMm = heightMm;
            StoredBookTitle = null;
            CurrentStock = 0;
            MaxCapacity = 0; // 초기에는 0, 책이 입고될 때 계산됨
        }

        /// <summary>
        /// 책을 입고하기 전에 용량 및 높이 제한을 검증합니다.
        /// </summary>
        /// <param name="book">입고할 책 정보</param>
        /// <param name="quantity">입고할 수량</param>
        /// <param name="errorCode">오류 발생 시 오류 코드</param>
        /// <returns>입고 가능 여부</returns>
        public bool CanPutBook(Book book, int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE; // 기본값

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                Debug.LogWarning($"[Cell] {errorCode.ToMessage()}");
                return false;
            }

            // 1. 높이 제한 검증 (AC-11)
            if (book.HeightMm > HeightMm)
            {
                errorCode = ErrorCode.HEIGHT_LIMIT;
                string detailedMessage = ErrorMessageFormatter.FormatHeightError(CellCode, book.Title, book.HeightMm, HeightMm);
                Debug.LogWarning($"[Cell] {detailedMessage}");
                return false;
            }

            // 2. 기존 책과의 일치 여부 검증 (칸이 비어있지 않은 경우)
            if (!IsEmpty && StoredBookTitle != book.Title)
            {
                errorCode = ErrorCode.BOOK_MISMATCH;
                string detailedMessage = ErrorMessageFormatter.FormatBookMismatchError(CellCode, StoredBookTitle, book.Title);
                Debug.LogWarning($"[Cell] {detailedMessage}");
                return false;
            }

            // 3. 용량 계산 및 검증
            if (IsEmpty) // 칸이 비어있으면 새로운 책 기준으로 용량 계산
            {
                MaxCapacity = Mathf.FloorToInt((float)WidthMm / book.ThicknessMm);
                if (MaxCapacity == 0) // 책 두께가 칸 너비보다 커서 한 권도 못 들어가는 경우
                {
                    errorCode = ErrorCode.CAPACITY_FULL;
                    string detailedMessage = ErrorMessageFormatter.FormatCapacityError(CellCode, CurrentStock, MaxCapacity, quantity);
                    Debug.LogWarning($"[Cell] {detailedMessage}");
                    return false;
                }
            }
            // 칸이 비어있지 않으면 이미 MaxCapacity가 계산되어 있음

            // 4. 입고 제한 검증 (current + quantity <= capacity) - AC-10, AC-10.1
            int remainingCapacity = MaxCapacity - CurrentStock;
            if (quantity > remainingCapacity)
            {
                // AC-10.1: 잔여 용량이 quantity 미만이면 부분 적재 없이 전체 실패
                errorCode = ErrorCode.CAPACITY_FULL;
                string detailedMessage = ErrorMessageFormatter.FormatCapacityError(CellCode, CurrentStock, MaxCapacity, quantity);
                Debug.LogWarning($"[Cell] {detailedMessage}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 책을 칸에 입고합니다. (CanPutBook 검증 후 호출)
        /// </summary>
        /// <param name="book">입고할 책 정보</param>
        /// <param name="quantity">입고할 수량</param>
        public void PutBook(Book book, int quantity)
        {
            if (IsEmpty)
            {
                StoredBookTitle = book.Title;
                MaxCapacity = Mathf.FloorToInt((float)WidthMm / book.ThicknessMm); // 다시 계산 (CanPutBook에서 이미 했지만 안전하게)
            }
            CurrentStock += quantity;
            Debug.Log($"[Cell] {CellCode}: {book.Title} {quantity}권 입고. 현재 {CurrentStock}/{MaxCapacity}권");
        }

        /// <summary>
        /// 책을 출고하기 전에 수량 제한을 검증합니다.
        /// </summary>
        /// <param name="quantity">출고할 수량</param>
        /// <param name="errorCode">오류 발생 시 오류 코드</param>
        /// <returns>출고 가능 여부</returns>
        public bool CanPickBook(int quantity, out ErrorCode errorCode)
        {
            errorCode = ErrorCode.NONE;

            if (quantity <= 0)
            {
                errorCode = ErrorCode.INVALID_VALUE;
                Debug.LogWarning($"[Cell] {errorCode.ToMessage()}");
                return false;
            }

            // 1. 출고 제한 검증 (current >= quantity) - AC-10.1 (출고 버전)
            if (CurrentStock < quantity)
            {
                errorCode = ErrorCode.INSUFFICIENT_STOCK;
                string detailedMessage = ErrorMessageFormatter.FormatInsufficientStockError(CellCode, StoredBookTitle, CurrentStock, quantity);
                Debug.LogWarning($"[Cell] {detailedMessage}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 책을 칸에서 출고합니다. (CanPickBook 검증 후 호출)
        /// </summary>
        /// <param name="quantity">출고할 수량</param>
        public void PickBook(int quantity)
        {
            string bookTitle = StoredBookTitle; // 로그 출력 전에 제목 저장
            int previousCapacity = MaxCapacity; // 용량도 저장

            CurrentStock -= quantity;
            if (CurrentStock == 0)
            {
                StoredBookTitle = null;
                MaxCapacity = 0; // 칸이 비었으므로 용량 초기화
            }

            Debug.Log($"[Cell] {CellCode}: {bookTitle} {quantity}권 출고. 현재 {CurrentStock}/{(CurrentStock == 0 ? previousCapacity : MaxCapacity)}권");
        }
    }
}
