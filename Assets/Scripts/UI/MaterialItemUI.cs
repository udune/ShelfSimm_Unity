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
        /// <param name="materialType">자재 타입 (선택)</param>
        public void SetData(string materialName, int quantity, string materialType = "")
        {
            if (titleText != null)
            {
                if (string.IsNullOrEmpty(materialType))
                {
                    titleText.text = $"{materialName} ({quantity})";
                }
                else
                {
                    titleText.text = $"{materialName} [{materialType}] ({quantity})";
                }
            }
        }
    }
}