using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Visualization3D
{
    public class Simulation3DWindow : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject windowPanel;
        public RawImage viewportImage;
        public TextMeshProUGUI statusText;

        [Header("3D References")]
        public Camera camera3D;
        public Simulation3DEnvironment environment;

        private bool isOpen = false;
        private RobotController robotController;

        void Start()
        {
            windowPanel.SetActive(false);
            Debug.Log("[Simulation3DWindow] Initialized");
        }

        public void Open(RobotController controller)
        {
            if (isOpen)
            {
                Debug.LogWarning("[Simulation3DWindow] Window is already open");
                return;
            }

            robotController = controller;
            Debug.Log("[Simulation3DWindow] Opening 3D window...");

            // 3D 환경 생성
            environment.Initialize();

            // 로봇 가져오기
            var robot = environment.GetRobot();

            // 카메라 타겟 설정
            var cameraFollow = camera3D.GetComponent<CameraFollow3D>();
            cameraFollow.SetTarget(robot.transform);

            // RobotController 로봇 초기화
            robot.Initialize(robotController);
            robotController.OnStatusChanged += UpdateStatus;

            // 창 열기
            windowPanel.SetActive(true);

            isOpen = true;
            Debug.Log("[Simulation3DWindow] 3D window opened successfully");
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            Debug.Log("[Simulation3DWindow] Closing 3D window...");

            // 이벤트 구독 해제
            robotController.OnStatusChanged -= UpdateStatus;

            // 3D 환경 정리
            environment.Cleanup();

            // 창 닫기
            windowPanel.SetActive(false);

            isOpen = false;
            robotController = null;

            Debug.Log("[Simulation3DWindow] 3D window closed");
        }

        private void UpdateStatus(string status)
        {
            statusText.text = status;
        }
    }
}