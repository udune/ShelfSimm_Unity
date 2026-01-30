using UnityEngine;

namespace Visualization3D
{
    public class CameraFollow3D : MonoBehaviour
    {
        public Transform target;

        [Header("Camera Settings")]
        public Vector3 offset = new Vector3(-1, 2f, -1);
        public float smoothSpeed = 5f;
        public float rotationSpeed = 5f;

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // 로봇의 회전을 고려한 로컬 오프셋 계산 (로봇 뒤쪽에서 따라감)
            Vector3 desiredPosition = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );

            // 로봇을 바라보도록 회전
            Vector3 lookDirection = target.position - transform.position;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;

            if (target != null)
            {
                // 즉시 카메라를 올바른 위치로 이동 (부드럽게 이동하는 효과 제거)
                Vector3 initialPosition = target.position + target.rotation * offset;
                transform.position = initialPosition;

                // 즉시 로봇을 바라보도록 설정
                Vector3 lookDirection = target.position - transform.position;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }

                Debug.Log($"[CameraFollow3D] Target set to: {newTarget.name}, camera positioned at {initialPosition}");
            }
        }
    }
}