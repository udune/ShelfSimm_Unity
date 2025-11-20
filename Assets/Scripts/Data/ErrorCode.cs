namespace Data
{
    // 다양한 오류 코드를 나타내는 열거형
    public enum ErrorCode
    {
        NONE,               // 오류 없음
        CAPACITY_FULL,      // 칸이 가득 참
        HEIGHT_LIMIT,       // 높이 제한 초과
        ROUTE_BLOCKED,      // 경로 차단됨
        ROUTE_TIMEOUT,      // 이동 타임아웃
        BOOK_MISMATCH,      // 도서 불일치
        INVALID_CODE,       // 잘못된 칸 코드
        INVALID_LAYOUT,     // 잘못된 레이아웃
        DUPLICATE_CODE,     // 중복 코드
        OVERLAP_CELL,       // 칸 영역 겹침
        INVALID_VALUE,      // 잘못된 값
        CANCELLED_BY_STOP,  // 시뮬레이션 중지로 인한 취소
        INSUFFICIENT_STOCK, // 재고 부족
        ROBOT_BUSY,         // 로봇이 다른 작업 중
        CELL_LOCKED,        // 셀이 잠겨 있음 (동시성 제어)
        TRANSACTION_FAILED, // 트랜잭션 실패
        WAREHOUSE_INSUFFICIENT_STOCK // 창고 재고 부족
    }
    
    // ErrorCode 열거형에 대한 확장 메서드 클래스
    public static class ErrorCodeExtensions
    {
        // ErrorCode를 사용자 친화적인 메시지로 변환하는 메서드
        public static string ToMessage(this ErrorCode errorCode)
        {
            // 각 오류 코드에 대한 메시지 반환
            return errorCode switch
            {
                ErrorCode.NONE => "정상 처리",
                ErrorCode.CAPACITY_FULL => "칸의 잔여 용량이 부족하여 요청한 수량을 모두 입고할 수 없습니다 (부분 적재 불가)",
                ErrorCode.HEIGHT_LIMIT => "도서 높이(mm)가 칸 높이(mm)를 초과하여 입고할 수 없습니다",
                ErrorCode.ROUTE_BLOCKED => "접근 가능한 경로가 없습니다",
                ErrorCode.ROUTE_TIMEOUT => "이동 시간이 초과되었습니다",
                ErrorCode.BOOK_MISMATCH => "해당 칸에 다른 도서가 보관되어 있습니다",
                ErrorCode.INVALID_CODE => "알 수 없는 칸 코드입니다",
                ErrorCode.INVALID_LAYOUT => "잘못된 레이아웃입니다",
                ErrorCode.DUPLICATE_CODE => "중복된 칸 코드입니다",
                ErrorCode.OVERLAP_CELL => "칸 영역이 겹칩니다",
                ErrorCode.INVALID_VALUE => "잘못된 수량 값입니다 (양수 필요)",
                ErrorCode.CANCELLED_BY_STOP => "사용자에 의해 작업이 취소되었습니다",
                ErrorCode.INSUFFICIENT_STOCK => "칸의 현재 재고가 요청한 출고 수량보다 부족합니다 (부분 출고 불가)",
                ErrorCode.ROBOT_BUSY => "로봇이 현재 다른 작업을 수행 중입니다",
                ErrorCode.CELL_LOCKED => "해당 칸이 다른 작업에 의해 잠겨 있습니다",
                ErrorCode.TRANSACTION_FAILED => "트랜잭션 처리 중 오류가 발생했습니다",
                ErrorCode.WAREHOUSE_INSUFFICIENT_STOCK => "창고의 현재 재고가 요청한 입고 수량보다 부족합니다",
                _ => "알 수 없는 오류입니다"
            };
        }
    }
}
