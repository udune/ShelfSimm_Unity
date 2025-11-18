using TMPro;
using UnityEngine;

namespace UI
{
    public class CellInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cellCodeText;
        [SerializeField] private TextMeshProUGUI accessibilityText;
        [SerializeField] private GameObject panelObject;

        [Header("Colors")]
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;

        private void Start()
        {
            // 초기에는 패널 숨김
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }

        public void UpdateCellInfo(string cellCode, bool isAccessible)
        {
            // 패널 활성화
            if (panelObject != null)
            {
                panelObject.SetActive(true);
            }

            // 셀 코드 표시
            if (cellCodeText != null)
            {
                cellCodeText.text = $"셀: {cellCode}";
            }

            // 접근성 표시
            if (accessibilityText != null)
            {
                if (isAccessible)
                {
                    accessibilityText.text = "접근 가능";
                    accessibilityText.color = accessibleColor;
                }
                else
                {
                    accessibilityText.text = "접근 불가";
                    accessibilityText.color = blockedColor;
                }
            }

            Debug.Log($"[CellInfoPanel] 셀 정보 업데이트: {cellCode} | 접근: {isAccessible}");
        }

        public void Hide()
        {
            if (panelObject != null)
            {
                panelObject.SetActive(false);
            }
        }
    }
}
