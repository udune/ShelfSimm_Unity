using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    // 설정 구조체
    [Serializable]
    public struct GameSettings
    {
        public float FixedDeltaTime; // 고정된 델타 타임
        public int RandomSeed; // 랜덤 시드 값
        public bool EnableVSync; // VSync 활성화 여부
        public int TargetFrameRate; // 목표 프레임 레이트
    }
    
    // 초기화 클래스
    public class Initializer : MonoBehaviour
    {
        [Header("시뮬레이션 설정")] 
        [SerializeField] private float fixedDeltaTime = 0.02f; // 50 FPS
        [SerializeField] private int randomSeed = 42; // 고정된 시드 값
        [SerializeField] private bool enableVSync = false; // VSync 비활성화
        [SerializeField] private int targetFrameRate = 60; // 목표 프레임 레이트
        
        [Header("디버그 정보")]
        [SerializeField] private bool showDebugInfo = true; // 디버그 정보 표시 여부

        private void Awake()
        {
            DontDestroyOnLoad(this); // 씬 전환 시에도 파괴되지 않도록 설정
            
            Init(); // 초기화 함수 호출

            if (showDebugInfo)
            {
                Debug.Log($"[Initializer] Awake called at {DateTime.Now}");
            }
        }

        // 초기화 함수
        private void Init()
        {
            Time.fixedDeltaTime = fixedDeltaTime; // 고정된 델타 타임 설정
            
            Random.InitState(randomSeed); // 고정된 시드 값으로 랜덤 초기화
            
            QualitySettings.vSyncCount = enableVSync ? 1 : 0; // VSync 설정
            
            if (!enableVSync)
            {
                Application.targetFrameRate = targetFrameRate; // VSync가 비활성화된 경우에만 목표 프레임 레이트 설정
            }
            
            Application.runInBackground = true; // 백그라운드 실행 허용
        }

        // 랜덤 시드 값을 변경하는 함수
        public void SetRandomSeed(int seed)
        {
            randomSeed = seed; // 새로운 시드 값 설정
            Random.InitState(randomSeed); // 새로운 시드 값으로 랜덤 초기화
            
            if (showDebugInfo)
            {
                Debug.Log($"[Initializer] Random seed set to {randomSeed}");
            }
        }

        // 현재 설정 값을 반환하는 함수
        public GameSettings GetCurrentSettings()
        {
            return new GameSettings // 현재 설정 값을 구조체로 반환
            {
                FixedDeltaTime = fixedDeltaTime,
                RandomSeed = randomSeed,
                EnableVSync = enableVSync,
                TargetFrameRate = targetFrameRate
            };
        }
        
        #region Editor Utilities
        
        #if UNITY_EDITOR
        [Space(10)]
        [Header("에디터 유틸리티")]
        [SerializeField] private bool resetOnPlay = true; // 플레이 시 초기화 여부

        // 에디터에서 값이 변경될 때마다 호출
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                Time.fixedDeltaTime = fixedDeltaTime;
                Random.InitState(randomSeed);
            }
        }

        // 플레이 모드로 전환될 때 호출
        private void Start()
        {
            if (resetOnPlay)
            {
                Init(); // 플레이 시 초기화
            }
        }
        
        // 현재 설정 값을 출력하는 함수
        [ContextMenu("현재 설정 출력")]
        private void PrintCurrentSettings()
        {
            var settings = GetCurrentSettings();
            Debug.Log($"[Initializer] Current Settings:\n" +
                      $"- FixedDeltaTime: {settings.FixedDeltaTime}\n" +
                      $"- RandomSeed: {settings.RandomSeed}\n" +
                      $"- EnableVSync: {settings.EnableVSync}\n" +
                      $"- TargetFrameRate: {settings.TargetFrameRate}");
        }
#endif
        
        #endregion
    }
}
