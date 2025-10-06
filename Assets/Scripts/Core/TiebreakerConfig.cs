using UnityEngine;

namespace Core
{
    // 타이브레이커 설정
    [CreateAssetMenu(fileName = "TiebreakerConfig", menuName = "Scriptable Objects/TiebreakerConfig")]
    public class TiebreakerConfig : ScriptableObject
    {
        public enum TiebreakerMode // 타이브레이커 모드
        {
            Alphabetical, // 알파벳순
            Random        // 랜덤
        }

        [Header("타이브레이커 설정")] 
        public TiebreakerMode mode = TiebreakerMode.Alphabetical; // 동일 거리/비용 칸 존재 시 선택 방식

        public int randomSeed = 42; // 랜덤 모드 시 사용할 시드값 (기본: 42)
        
        [Header("로깅 설정")]
        public bool enableLogging = true; // 타이브레이커 결정 로그 출력
    }
}
