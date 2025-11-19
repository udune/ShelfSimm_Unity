using NUnit.Framework;
using Core.Core;
using Data;

namespace Tests
{
    /// <summary>
    /// ErrorMessageFormatter 테스트 (T-502)
    /// AC-10, AC-10.1, AC-11, AC-12, AC-12.1 관련 메시지 포맷팅 검증
    /// </summary>
    public class ErrorMessageFormatterTests
    {
        [Test]
        public void FormatCapacityError_ReturnsCorrectMessage()
        {
            // Arrange
            string cellCode = "A01";
            int currentStock = 5;
            int maxCapacity = 10;
            int requestedQuantity = 8;

            // Act
            string message = ErrorMessageFormatter.FormatCapacityError(cellCode, currentStock, maxCapacity, requestedQuantity);

            // Assert
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(cellCode));
            Assert.IsTrue(message.Contains("5/10")); // 현재/최대
            Assert.IsTrue(message.Contains("잔여 용량 5권")); // 잔여 용량
            Assert.IsTrue(message.Contains("요청 8권")); // 요청 수량
            Assert.IsTrue(message.Contains("부분 적재 불가")); // AC-10.1 검증
        }

        [Test]
        public void FormatCapacityError_WithZeroCapacity_ReturnsCorrectMessage()
        {
            // Arrange
            string cellCode = "B02";
            int currentStock = 0;
            int maxCapacity = 0;
            int requestedQuantity = 1;

            // Act
            string message = ErrorMessageFormatter.FormatCapacityError(cellCode, currentStock, maxCapacity, requestedQuantity);

            // Assert
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(cellCode));
            Assert.IsTrue(message.Contains("0/0")); // 용량 없음 (책 두께가 너무 큼)
        }

        [Test]
        public void FormatHeightError_ReturnsCorrectMessage_WithMmUnit()
        {
            // Arrange
            string cellCode = "C03";
            string bookTitle = "대형 사전";
            int bookHeight = 350;
            int cellHeight = 300;

            // Act
            string message = ErrorMessageFormatter.FormatHeightError(cellCode, bookTitle, bookHeight, cellHeight);

            // Assert (AC-11, AC-12.1)
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(cellCode));
            Assert.IsTrue(message.Contains(bookTitle));
            Assert.IsTrue(message.Contains("350mm")); // 도서 높이 (mm 단위)
            Assert.IsTrue(message.Contains("300mm")); // 칸 높이 (mm 단위)
            Assert.IsTrue(message.Contains("+50mm 초과")); // 초과 치수
            Assert.IsTrue(message.Contains("입고 불가"));
        }

        [Test]
        public void FormatInsufficientStockError_ReturnsCorrectMessage()
        {
            // Arrange
            string cellCode = "D04";
            string bookTitle = "테스트 도서";
            int currentStock = 3;
            int requestedQuantity = 5;

            // Act
            string message = ErrorMessageFormatter.FormatInsufficientStockError(cellCode, bookTitle, currentStock, requestedQuantity);

            // Assert (AC-10.1 출고 버전)
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(cellCode));
            Assert.IsTrue(message.Contains(bookTitle));
            Assert.IsTrue(message.Contains("현재 3권")); // 현재 재고
            Assert.IsTrue(message.Contains("요청 5권")); // 요청 수량
            Assert.IsTrue(message.Contains("2권 부족")); // 부족 수량
            Assert.IsTrue(message.Contains("부분 출고 불가")); // AC-10.1 검증
        }

        [Test]
        public void FormatBookMismatchError_ReturnsCorrectMessage()
        {
            // Arrange
            string cellCode = "E05";
            string storedBookTitle = "기존 도서";
            string requestedBookTitle = "신규 도서";

            // Act
            string message = ErrorMessageFormatter.FormatBookMismatchError(cellCode, storedBookTitle, requestedBookTitle);

            // Assert
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains(cellCode));
            Assert.IsTrue(message.Contains(storedBookTitle));
            Assert.IsTrue(message.Contains(requestedBookTitle));
            Assert.IsTrue(message.Contains("불일치"));
            Assert.IsTrue(message.Contains("입고 불가"));
        }

        [Test]
        public void FormatCellInfo_WithStock_ReturnsCorrectFormat()
        {
            // Arrange
            string cellCode = "F06";
            int widthMm = 900;
            int heightMm = 300;
            int currentStock = 25;
            int maxCapacity = 30;
            string storedBookTitle = "프로그래밍 언어론";

            // Act
            string info = ErrorMessageFormatter.FormatCellInfo(cellCode, widthMm, heightMm, currentStock, maxCapacity, storedBookTitle);

            // Assert (AC-12, AC-12.1)
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Contains(cellCode));
            Assert.IsTrue(info.Contains("900mm")); // 너비 (mm 단위)
            Assert.IsTrue(info.Contains("300mm")); // 높이 (mm 단위)
            Assert.IsTrue(info.Contains("25/30권")); // 재고 정보
            Assert.IsTrue(info.Contains(storedBookTitle)); // 도서 제목
        }

        [Test]
        public void FormatCellInfo_EmptyCell_ReturnsCorrectFormat()
        {
            // Arrange
            string cellCode = "G07";
            int widthMm = 1200;
            int heightMm = 400;
            int currentStock = 0;
            int maxCapacity = 0;
            string storedBookTitle = null;

            // Act
            string info = ErrorMessageFormatter.FormatCellInfo(cellCode, widthMm, heightMm, currentStock, maxCapacity, storedBookTitle);

            // Assert (AC-12, AC-12.1)
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Contains(cellCode));
            Assert.IsTrue(info.Contains("1200mm")); // 너비 (mm 단위)
            Assert.IsTrue(info.Contains("400mm")); // 높이 (mm 단위)
            Assert.IsTrue(info.Contains("빈 칸")); // 빈 칸 표시
            Assert.IsTrue(info.Contains("없음")); // 도서 없음
        }

        [Test]
        public void ErrorCodeExtensions_ToMessage_ContainsMmUnit()
        {
            // Arrange & Act
            string heightMessage = ErrorCode.HEIGHT_LIMIT.ToMessage();
            string capacityMessage = ErrorCode.CAPACITY_FULL.ToMessage();
            string stockMessage = ErrorCode.INSUFFICIENT_STOCK.ToMessage();

            // Assert (AC-11, AC-12.1)
            Assert.IsTrue(heightMessage.Contains("(mm)")); // mm 단위 명시
            Assert.IsTrue(capacityMessage.Contains("부분 적재 불가")); // AC-10.1
            Assert.IsTrue(stockMessage.Contains("부분 출고 불가")); // AC-10.1
        }

        [Test]
        public void ErrorCodeExtensions_ToMessage_AllCodesHaveMessages()
        {
            // Arrange
            var allErrorCodes = new[]
            {
                ErrorCode.NONE,
                ErrorCode.CAPACITY_FULL,
                ErrorCode.HEIGHT_LIMIT,
                ErrorCode.ROUTE_BLOCKED,
                ErrorCode.ROUTE_TIMEOUT,
                ErrorCode.BOOK_MISMATCH,
                ErrorCode.INVALID_CODE,
                ErrorCode.INVALID_LAYOUT,
                ErrorCode.DUPLICATE_CODE,
                ErrorCode.OVERLAP_CELL,
                ErrorCode.INVALID_VALUE,
                ErrorCode.CANCELLED_BY_STOP,
                ErrorCode.INSUFFICIENT_STOCK,
                ErrorCode.ROBOT_BUSY
            };

            // Act & Assert
            foreach (var errorCode in allErrorCodes)
            {
                string message = errorCode.ToMessage();
                Assert.IsNotNull(message);
                Assert.IsNotEmpty(message);
                Assert.IsFalse(message.Contains("알 수 없는")); // 모든 코드가 정의된 메시지를 가져야 함
            }
        }
    }
}
