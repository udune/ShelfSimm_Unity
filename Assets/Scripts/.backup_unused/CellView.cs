using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CellView : MonoBehaviour
    {
        private Image image;
        private string currentType = "empty";

        public void Init(int x, int y)
        {
            image = GetComponent<Image>();
            Render();
        }

        public bool UpdateCell(string type, string info)
        {
            if (currentType != type)
            {
                currentType = type;
                return true;
            }

            return false;
        }

        public void Render()
        {
            image.color = currentType switch
            {
                "empty" => new Color(0.8f, 0.8f, 0.8f),
                "partial" => Color.yellow,
                "full" => Color.red,
                "obstacle" => new Color(0.2f, 0.2f, 0.2f),
                "bookshelf" => new Color(0.55f, 0.43f, 0.39f),
                "robot" => Color.blue,
                _ => Color.white
            };
        }
    }
}
