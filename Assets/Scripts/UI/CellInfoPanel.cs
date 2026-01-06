using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;
using Utils;

namespace UI
{
    public class CellInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cellCodeText;
        [SerializeField] private TextMeshProUGUI accessibilityText;
        [SerializeField] private TextMeshProUGUI dimensionsText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private Slider capacitySlider;

        [Header("Material List")]
        [SerializeField] private MaterialItemUI materialItemPrefab;
        [SerializeField] private Transform materialListContent;

        [Header("Colors")]
        [SerializeField] private Color accessibleColor = Color.green;
        [SerializeField] private Color blockedColor = Color.red;

        private ObjectPool<MaterialItemUI> materialItemPool;

        private void Awake()
        {
            if (materialItemPrefab != null && materialListContent != null)
            {
                materialItemPool = new ObjectPool<MaterialItemUI>(materialItemPrefab, materialListContent, 5);
            }
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void UpdateCellInfo(string cellCode, bool isAccessible)
        {
            gameObject.SetActive(true);
            cellCodeText.text = $"{cellCode}";

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

        public void UpdateCellInfoDetailed(Cell cell, bool isAccessible)
        {
            cellCodeText.text = $"{cell.CellCode}";

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

            dimensionsText.text = $"{cell.WidthMm}mm × {cell.HeightMm}mm";

            // Object Pool을 사용한 자재 리스트 표시
            if (materialItemPool != null)
            {
                materialItemPool.ReturnAll();

                var allMaterials = cell.GetAllMaterials();
                foreach (var material in allMaterials)
                {
                    MaterialItemUI item = materialItemPool.Get();
                    item.SetData(material.name, material.quantity);
                }
            }

            // 용량 정보 업데이트 (항상 "현재/최대권" 형태로 표시)
            int currentStock = cell.CurrentStock;
            int maxCapacity = cell.MaxCapacity;

            capacityText.text = maxCapacity > 0 ? $"{currentStock}/{maxCapacity}권" : $"{currentStock} / -";

            // Slider 업데이트
            if (capacitySlider != null)
            {
                if (maxCapacity > 0)
                {
                    capacitySlider.value = (float)currentStock / maxCapacity;
                }
                else
                {
                    capacitySlider.value = 0f;
                }
            }
        }
    }
}
