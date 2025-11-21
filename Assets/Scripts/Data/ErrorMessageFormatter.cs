namespace Data
{
    public static class ErrorMessageFormatter
    {
        public static string FormatCapacityError(string cellCode, int currentStock, int maxCapacity, int requestedQuantity)
        {
            int remainingCapacity = maxCapacity - currentStock;
            return $"[{cellCode}] 용량 초과: 현재 {currentStock}/{maxCapacity}권, 잔여 용량 {remainingCapacity}권, 요청 {requestedQuantity}권 " +
                   $"→ 부분 적재 불가로 전체 실패 처리됨";
        }

        public static string FormatHeightError(string cellCode, string bookTitle, int bookHeight, int cellHeight)
        {
            int heightDifference = bookHeight - cellHeight;
            return $"[{cellCode}] 높이 초과: 도서 '{bookTitle}' 높이 {bookHeight}mm > 칸 높이 {cellHeight}mm " +
                   $"(+{heightDifference}mm 초과) → 입고 불가";
        }

        public static string FormatInsufficientStockError(string cellCode, string bookTitle, int currentStock, int requestedQuantity)
        {
            int shortage = requestedQuantity - currentStock;
            return $"[{cellCode}] 재고 부족: '{bookTitle}' 현재 {currentStock}권, 요청 {requestedQuantity}권 " +
                   $"({shortage}권 부족) → 부분 출고 불가로 전체 실패 처리됨";
        }

        public static string FormatBookMismatchError(string cellCode, string storedBookTitle, string requestedBookTitle)
        {
            return $"[{cellCode}] 도서 불일치: 보관 중인 도서 '{storedBookTitle}' ≠ 요청 도서 '{requestedBookTitle}' → 입고 불가";
        }

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
