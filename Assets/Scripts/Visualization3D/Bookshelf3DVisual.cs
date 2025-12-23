using UnityEngine;

namespace Visualization3D
{
    public class Bookshelf3DVisual : MonoBehaviour
    {
        private string cellCode;
        private Material material;

        public void Initialize(string code)
        {
            cellCode = code;
            material = GetComponent<Renderer>().material;
        }
        
        public void UpdateVisual(bool isEmpty, bool isFull)
        {
            if (isEmpty)
            {
                material.color = new Color(0.7f, 0.7f, 0.7f); // 회색
            }
            else if (isFull)
            {
                material.color = Color.red; // 빨간색
            }
            else
            {
                material.color = Color.yellow; // 노란색
            }
        }
    }
}