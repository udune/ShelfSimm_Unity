using System.Collections;
using Core;
using UnityEngine;

namespace Visualization3D
{
    public class Robot3DVisual : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;

        private RobotController controller;
        private Coroutine moveCoroutine;

        public void Initialize(RobotController ctrl)
        {
            controller = ctrl;
            controller.OnPositionChanged += OnPositionChanged;

            Debug.Log("[Robot3DVisual] Initialized and subscribed to position changes");
        }

        private void OnPositionChanged(Vector2Int gridPos)
        {
            Vector3 targetPos = GridToWorld(gridPos);

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            moveCoroutine = StartCoroutine(SmoothMove(targetPos));
        }

        private IEnumerator SmoothMove(Vector3 targetPos)
        {
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                // 이동 방향으로 회전
                Vector3 direction = (targetPos - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        10f * Time.deltaTime
                    );
                }

                yield return null;
            }

            transform.position = targetPos;
        }

        private Vector3 GridToWorld(Vector2Int gridPos)
        {
            const float GRID_SCALE = 2f;
            return new Vector3(gridPos.x * GRID_SCALE, 0, gridPos.y * GRID_SCALE);
        }

        private void OnDestroy()
        {
            controller.OnPositionChanged -= OnPositionChanged;
        }
    }
}