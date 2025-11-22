using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [Serializable]
    public struct GameSettings
    {
        public float FixedDeltaTime;
        public int RandomSeed;
        public bool EnableVSync;
        public int TargetFrameRate;
    }

    public class Initializer : MonoBehaviour
    {
        [Header("시뮬레이션 설정")]
        [SerializeField] private float fixedDeltaTime = 0.02f;
        [SerializeField] private int randomSeed = 42;
        [SerializeField] private bool enableVSync = false;
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Init();
        }

        private void Init()
        {
            Time.fixedDeltaTime = fixedDeltaTime;
            Random.InitState(randomSeed);
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;

            if (!enableVSync)
            {
                Application.targetFrameRate = targetFrameRate;
            }

            Application.runInBackground = true;
        }

        public void SetRandomSeed(int seed)
        {
            randomSeed = seed;
            Random.InitState(randomSeed);
        }

        public GameSettings GetCurrentSettings()
        {
            return new GameSettings
            {
                FixedDeltaTime = fixedDeltaTime,
                RandomSeed = randomSeed,
                EnableVSync = enableVSync,
                TargetFrameRate = targetFrameRate
            };
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                Time.fixedDeltaTime = fixedDeltaTime;
                Random.InitState(randomSeed);
            }
        }
        #endif
    }
}
