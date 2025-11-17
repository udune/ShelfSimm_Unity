namespace Core
{
    public enum RobotState
    {
        IDLE,       // 대기
        MOVING,     // 이동 중
        HANDLING,    // 작업 처리 중 (PUT/PICK)
        RETURNING
    }
}
