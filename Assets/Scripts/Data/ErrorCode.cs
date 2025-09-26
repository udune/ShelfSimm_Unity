namespace Data
{
    // 다양한 오류 코드를 나타내는 열거형
    public enum ErrorCode
    {
        CAPACITY_FULL,      // 칸이 가득 참
        HEIGHT_LIMIT,       // 높이 제한 초과
        ROUTE_BLOCKED,      // 경로 차단됨
        ROUTE_TIMEOUT,      // 이동 타임아웃
        BOOK_MISMATCH,      // 도서 불일치
        INVALID_CODE,       // 잘못된 칸 코드
        INVALID_LAYOUT,     // 잘못된 레이아웃
        DUPLICATE_CODE,     // 중복 코드
        OVERLAP_CELL,       // 칸 영역 겹침
        INVALID_VALUE       // 잘못된 값
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
                ErrorCode.CAPACITY_FULL => "이 칸은 가득 찼습니다",
                ErrorCode.HEIGHT_LIMIT => "도서 높이가 칸 높이보다 큽니다",
                ErrorCode.ROUTE_BLOCKED => "접근 가능한 경로가 없습니다",
                ErrorCode.ROUTE_TIMEOUT => "이동 시간이 초과되었습니다",
                ErrorCode.BOOK_MISMATCH => "해당 칸의 도서와 일치하지 않습니다",
                ErrorCode.INVALID_CODE => "알 수 없는 코드입니다",
                ErrorCode.INVALID_LAYOUT => "잘못된 레이아웃입니다",
                ErrorCode.DUPLICATE_CODE => "중복된 코드입니다",
                ErrorCode.OVERLAP_CELL => "칸 영역이 겹칩니다",
                ErrorCode.INVALID_VALUE => "잘못된 값입니다",
                _ => "알 수 없는 오류입니다"
            };
        }
    }
}
