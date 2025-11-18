using System;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "Scriptable Objects/SimulationConfig")]
    public class SimulationConfig : ScriptableObject
    {
        public event Action<float> OnHandleTimeChanged;

        [Header("작업 처리 시간")]
        [SerializeField, Min(0.1f)]
        private float _handleTime = 2.0f;
        public float handleTime
        {
            get => _handleTime;
            set
            {
                if (Math.Abs(_handleTime - value) > 0.001f && value > 0)
                {
                    _handleTime = value;
                    OnHandleTimeChanged?.Invoke(_handleTime);
                }
            }
        }

        [Header("로봇 이동 설정")]
        [Min(0.1f)]
        public float robotSpeed = 3.0f;

        [Min(1.0f)]
        public float moveTimeoutSec = 30f;

        [Header("경로 탐색 설정")]
        [Range(1, 10)]
        public int topN = 3;

        [Header("결정성 설정")]
        public int randomSeed = 42;
        
        [Header("창고 설정")]
        public Vector2Int warehousePos = Vector2Int.zero;
    }
}
