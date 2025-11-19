using Data;

namespace Core
{
    /// <summary>
    /// 에러 메시지를 상세 정보와 함께 포맷팅하는 유틸리티 클래스
    /// AC-10, AC-10.1, AC-11, AC-12.1 관련 메시지 생성
    /// </summary>
    public static class ErrorMessageFormatter
    {
        /// <summary>
        /// 용량 초과 에러에 대한 상세 메시지 생성 (AC-10, AC-10.1)
        /// </summary>
        /// <param name="cellCode">칸 코드</param>
        /// <param name="currentStock">현재 재고</param>
        /// <param name="maxCapacity">최대 용량</param>
        /// <param name="requestedQuantity">요청 수량</param>
        /// <returns>상세 에러 메시지</returns>
        public static string FormatCapacityError(string cellCode, int currentStock, int maxCapacity, int requestedQuantity)
        {
            int remainingCapacity = maxCapacity - currentStock;
            return $"[{cellCode}] 용량 초과: 현재 {currentStock}/{maxCapacity}권, 잔여 용량 {remainingCapacity}권, 요청 {requestedQuantity}권 " +
                   $"→ 부분 적재 불가로 전체 실패 처리됨";
        }

        /// <summary>
        /// 높이 초과 에러에 대한 상세 메시지 생성 (AC-11, AC-12.1)
        /// </summary>
        /// <param name="cellCode">칸 코드</param>
        /// <param name="bookTitle">도서 제목</param>
        /// <param name="bookHeight">도서 높이 (mm)</param>
        /// <param name="cellHeight">칸 높이 (mm)</param>
        /// <returns>상세 에러 메시지</returns>
        public static string FormatHeightError(string cellCode, string bookTitle, int bookHeight, int cellHeight)
        {
            int heightDifference = bookHeight - cellHeight;
            return $"[{cellCode}] 높이 초과: 도서 '{bookTitle}' 높이 {bookHeight}mm > 칸 높이 {cellHeight}mm " +
                   $"(+{heightDifference}mm 초과) → 입고 불가";
        }

        /// <summary>
        /// 재고 부족 에러에 대한 상세 메시지 생성 (AC-10.1 출고 버전)
        /// </summary>
        /// <param name="cellCode">칸 코드</param>
        /// <param name="bookTitle">도서 제목</param>
        /// <param name="currentStock">현재 재고</param>
        /// <param name="requestedQuantity">요청 수량</param>
        /// <returns>상세 에러 메시지</returns>
        public static string FormatInsufficientStockError(string cellCode, string bookTitle, int currentStock, int requestedQuantity)
        {
            int shortage = requestedQuantity - currentStock;
            return $"[{cellCode}] 재고 부족: '{bookTitle}' 현재 {currentStock}권, 요청 {requestedQuantity}권 " +
                   $"({shortage}권 부족) → 부분 출고 불가로 전체 실패 처리됨";
        }

        /// <summary>
        /// 도서 불일치 에러에 대한 상세 메시지 생성
        /// </summary>
        /// <param name="cellCode">칸 코드</param>
        /// <param name="storedBookTitle">현재 보관 중인 도서</param>
        /// <param name="requestedBookTitle">요청된 도서</param>
        /// <returns>상세 에러 메시지</returns>
        public static string FormatBookMismatchError(string cellCode, string storedBookTitle, string requestedBookTitle)
        {
            return $"[{cellCode}] 도서 불일치: 보관 중인 도서 '{storedBookTitle}' ≠ 요청 도서 '{requestedBookTitle}' → 입고 불가";
        }

        /// <summary>
        /// 칸 정보를 포맷팅 (AC-12, AC-12.1 - 용량/치수 표시)
        /// </summary>
        /// <param name="cellCode">칸 코드</param>
        /// <param name="widthMm">칸 너비 (mm)</param>
        /// <param name="heightMm">칸 높이 (mm)</param>
        /// <param name="currentStock">현재 재고</param>
        /// <param name="maxCapacity">최대 용량</param>
        /// <param name="storedBookTitle">보관 중인 도서 (null이면 빈 칸)</param>
        /// <returns>포맷팅된 칸 정보</returns>
        public static string FormatCellInfo(string cellCode, int widthMm, int heightMm, int currentStock, int maxCapacity, string storedBookTitle)
        {
            string stockInfo = maxCapacity > 0
                ? $"{currentStock}/{maxCapacity}권"
                : "빈 칸";

            string bookInfo = !string.IsNullOrEmpty(storedBookTitle)
                ? $"'{storedBookTitle}'"
                : "없음";

            return $"[{cellCode}] 치수: {widthMm}mm × {heightMm}mm | 재고: {stockInfo} | 보관 도서: {bookInfo}";
        }
    }
}
