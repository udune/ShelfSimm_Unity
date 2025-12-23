using UnityEngine;

namespace Visualization3D
{
    public class CameraFollow3D : MonoBehaviour
    {
        public Transform target;

        [Header("Camera Settings")]
        public Vector3 offset = new Vector3(0, 15, -15);
        public float smoothSpeed = 5f;
        public float lookAheadDistance = 3f;

        void LateUpdate()
        {
            // 로봇 위치 + 오프셋
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );

            // 로봇 약간 앞쪽을 바라봄
            Vector3 lookAtPoint = target.position + target.forward * lookAheadDistance;
            transform.LookAt(lookAtPoint);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            Debug.Log($"[CameraFollow3D] Target set to: {newTarget?.name ?? "null"}");
        }
    }
}