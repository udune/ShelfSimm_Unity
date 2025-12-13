using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RobotVisual : MonoBehaviour
    {
        [SerializeField] private Image robotImage;
        [SerializeField] private Color robotColor = Color.blue;

        private void Start()
        {
            if (robotImage != null)
            {
                robotImage.color = robotColor;
            }
        }
    }
}
