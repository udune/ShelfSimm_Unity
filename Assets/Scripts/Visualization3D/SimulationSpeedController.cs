using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization3D
{
    public class SimulationSpeedController : MonoBehaviour
    {
        public static SimulationSpeedController Instance { get; private set; }

        [Header("UI References")]
        public Slider speedSlider;
        public TextMeshProUGUI speedText;

        [Header("Speed Settings")]
        public float minSpeed = 0.1f;
        public float maxSpeed = 5f;
        public float defaultSpeed = 0.5f;

        private float currentSpeed = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            SetSpeed(defaultSpeed);
            speedSlider.minValue = minSpeed;
            speedSlider.maxValue = maxSpeed;
            speedSlider.value = defaultSpeed;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }

        private void OnSpeedChanged(float value)
        {
            SetSpeed(value);
        }

        public void SetSpeed(float speed)
        {
            currentSpeed = Mathf.Clamp(speed, minSpeed, maxSpeed);
            Time.timeScale = currentSpeed;

            speedText.text = $"속도: {currentSpeed:F1}x";

            Debug.Log($"Simulation speed set to {currentSpeed}x");
        }

        public float GetSpeed() => currentSpeed;
    }
}