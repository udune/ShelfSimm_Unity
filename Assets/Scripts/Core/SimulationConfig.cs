using UnityEngine;

namespace Core
{
    // 시뮬레이션 설정을 위한 ScriptableObject
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "Scriptable Objects/SimulationConfig")]
    public class SimulationConfig : ScriptableObject
    {
        [Header("작업 처리 시간")] 
        [Min(0.1f)] // 최소 0.1초 이상
        public float handleTime = 2.0f; // 작업 처리 시간 (초)
        
        [Header("로봇 이동 설정")]
        [Min(0.1f)] // 최소 0.1 유닛/초 이상
        public float robotSpeed = 3.0f; // 로봇 이동 속도 (유닛/초)

        [Min(1.0f)] // 최소 1초 이상
        public float moveTimeoutSec = 30f; // 로봇 이동 타임아웃 (초)

        [Header("경로 탐색 설정")] 
        [Range(1, 10)] // 1에서 10 사이의 값
        public int topN = 3; // 상위 N개의 경로를 고려

        [Header("결정성 설정")] 
        public int randomSeed = 42; // 랜덤 시드 값
        
        [Header("창고 설정")]
        public Vector2Int warehousePos = Vector2Int.zero; // 창고 위치 (2D 좌표)
    }
}
