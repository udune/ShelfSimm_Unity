using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// MaterialItemPrefab의 데이터를 표시하는 스크립트
    /// Object Pool로 재사용되는 자재 리스트 아이템
    /// </summary>
    public class MaterialItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;

        private void Awake()
        {
            if (titleText == null)
            {
                titleText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 자재 정보를 표시합니다.
        /// </summary>
        /// <param name="materialName">자재 이름</param>
        /// <param name="quantity">수량</param>
        public void SetData(string materialName, int quantity)
        {
            if (titleText != null)
            {
                titleText.text = $"{materialName} ({quantity}권)";
            }
        }
    }
}