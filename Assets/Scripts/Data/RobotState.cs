using UnityEngine;

namespace Data
{
    public enum RobotState
    {
        IDLE,// 대기
        MOVING, // 이동 중
        HANDLING, // 작업 처리 중
        RETURNING // 창고로 복귀 중
    }
}
