using UnityEngine;

namespace Visualization3D
{
    public class Bookshelf3DVisual : MonoBehaviour
    {
        private string cellCode;
        private Renderer renderer;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        public void Initialize(string code)
        {
            cellCode = code;
            renderer = GetComponentInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();

            // WebGL compatibility: Log if renderer is missing
            if (renderer == null)
            {
                Debug.LogError($"[Bookshelf3DVisual] No renderer found for {code}");
            }
        }

        public void UpdateVisual(bool isEmpty, bool isFull)
        {
            if (renderer == null) return;

            Color color;
            if (isEmpty)
            {
                color = new Color(0.7f, 0.7f, 0.7f); // 회색
            }
            else if (isFull)
            {
                color = Color.red; // 빨간색
            }
            else
            {
                color = Color.yellow; // 노란색
            }

            // Use MaterialPropertyBlock for WebGL compatibility
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}