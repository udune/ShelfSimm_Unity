using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "TiebreakerConfig", menuName = "Scriptable Objects/TiebreakerConfig")]
    public class TiebreakerConfig : ScriptableObject
    {
        public enum TiebreakerMode
        {
            Alphabetical,
            Random
        }

        [Header("타이브레이커 설정")]
        public TiebreakerMode mode = TiebreakerMode.Alphabetical;
        public int randomSeed = 42;

        [Header("로깅 설정")]
        public bool enableLogging = true;
    }
}
