using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using API;

namespace UI
{
    public class MaterialInventoryPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private TMP_Dropdown statusDropdown;
        [SerializeField] private Button refreshButton;

        [Header("Table")]
        [SerializeField] private Transform tableBody;
        [SerializeField] private GameObject rowPrefab;

        [Header("Pagination")]
        [SerializeField] private TextMeshProUGUI pageInfoText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Transform pageButtonContainer;
        [SerializeField] private GameObject pageButtonPrefab;

        private List<MaterialDto> allMaterials = new List<MaterialDto>();
        private int currentPage = 1;
        private int pageSize = 6;
        private int totalCount = 0;
        private bool isVisible = false;

        private void Start()
        {
            Hide();
            SetupEventListeners();
        }

        private void SetupEventListeners()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(OnRefreshClicked);
            if (prevButton != null)
                prevButton.onClick.AddListener(OnPrevPage);
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextPage);
        }

        public void Show()
        {
            isVisible = true;
            if (panelRoot != null)
                panelRoot.SetActive(true);

            LoadMaterials();
            Debug.Log("[MaterialInventoryPanel] Shown");
        }

        public void Hide()
        {
            isVisible = false;
            if (panelRoot != null)
                panelRoot.SetActive(false);

            Debug.Log("[MaterialInventoryPanel] Hidden");
        }

        public bool IsVisible => isVisible;

        private void OnRefreshClicked()
        {
            Debug.Log("[MaterialInventoryPanel] Refresh clicked");
            currentPage = 1;
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            if (ApiClient.Instance == null)
            {
                Debug.LogWarning("[MaterialInventoryPanel] ApiClient not found");
                return;
            }

            StartCoroutine(ApiClient.Instance.GetAllMaterials(
                onSuccess: (materials) =>
                {
                    allMaterials = materials;
                    totalCount = materials.Count;
                    Debug.Log($"[MaterialInventoryPanel] Loaded {totalCount} materials");
                    UpdateTable();
                    UpdatePagination();
                },
                onError: (error) =>
                {
                    Debug.LogError($"[MaterialInventoryPanel] Load failed: {error}");
                }
            ));
        }

        private void UpdateTable()
        {
            if (tableBody == null) return;

            // 기존 Row 삭제
            foreach (Transform child in tableBody)
            {
                Destroy(child.gameObject);
            }

            // 현재 페이지 데이터 계산
            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Mathf.Min(startIndex + pageSize, allMaterials.Count);

            // Row 생성
            for (int i = startIndex; i < endIndex; i++)
            {
                CreateRow(allMaterials[i]);
            }
        }

        private void CreateRow(MaterialDto material)
        {
            if (rowPrefab == null || tableBody == null) return;

            GameObject row = Instantiate(rowPrefab, tableBody);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            // 만료 여부 계산
            bool isExpired = false;
            if (!string.IsNullOrEmpty(material.expiryDate))
            {
                if (DateTime.TryParse(material.expiryDate, out DateTime expiry))
                {
                    isExpired = expiry < DateTime.Now;
                }
            }
            string status = isExpired ? "Expired" : "Warehouse";

            // Row 데이터 설정 (texts 배열 순서에 맞게)
            if (texts.Length >= 7)
            {
                texts[0].text = material.id ?? "";
                texts[1].text = material.name ?? "";
                texts[2].text = material.vendor ?? "";
                texts[3].text = material.lotId ?? "";
                texts[4].text = material.type ?? "";
                texts[5].text = $"{material.stockQty} L";
                texts[6].text = status;

                // 만료된 항목 스타일
                if (isExpired)
                {
                    texts[5].color = new Color(0.973f, 0.424f, 0.424f); // Red
                    texts[6].color = new Color(0.973f, 0.424f, 0.424f); // Red
                }
            }
        }

        private void UpdatePagination()
        {
            int totalPages = Mathf.CeilToInt((float)totalCount / pageSize);
            if (totalPages == 0) totalPages = 1;

            // 페이지 정보 텍스트
            int startItem = totalCount > 0 ? (currentPage - 1) * pageSize + 1 : 0;
            int endItem = Mathf.Min(currentPage * pageSize, totalCount);

            if (pageInfoText != null)
            {
                pageInfoText.text = $"Showing {startItem} to {endItem} of {totalCount} materials";
            }

            // Prev/Next 버튼 상태
            if (prevButton != null)
                prevButton.interactable = currentPage > 1;
            if (nextButton != null)
                nextButton.interactable = currentPage < totalPages;

            // 페이지 버튼 생성
            UpdatePageButtons(totalPages);
        }

        private void UpdatePageButtons(int totalPages)
        {
            if (pageButtonContainer == null) return;

            // 기존 버튼 삭제
            foreach (Transform child in pageButtonContainer)
            {
                Destroy(child.gameObject);
            }

            if (pageButtonPrefab == null) return;

            // 표시할 페이지 범위 계산 (최대 5개)
            int startPage = Mathf.Max(1, currentPage - 2);
            int endPage = Mathf.Min(totalPages, startPage + 4);

            // startPage 재조정 (끝 페이지 근처일 때)
            if (endPage - startPage < 4)
            {
                startPage = Mathf.Max(1, endPage - 4);
            }

            for (int i = startPage; i <= endPage; i++)
            {
                int pageNum = i;
                GameObject btn = Instantiate(pageButtonPrefab, pageButtonContainer);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = pageNum.ToString();
                }

                var button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => GoToPage(pageNum));
                }

                // 현재 페이지 하이라이트
                if (pageNum == currentPage)
                {
                    var image = btn.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = new Color(0.075f, 0.498f, 0.925f, 0.3f); // Primary Blue 20%
                    }
                    if (text != null)
                    {
                        text.color = new Color(0.075f, 0.498f, 0.925f); // Primary Blue
                        text.fontStyle = FontStyles.Bold;
                    }
                }
            }
        }

        private void OnPrevPage()
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateTable();
                UpdatePagination();
                Debug.Log($"[MaterialInventoryPanel] Page: {currentPage}");
            }
        }

        private void OnNextPage()
        {
            int totalPages = Mathf.CeilToInt((float)totalCount / pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateTable();
                UpdatePagination();
                Debug.Log($"[MaterialInventoryPanel] Page: {currentPage}");
            }
        }

        private void GoToPage(int page)
        {
            if (page != currentPage)
            {
                currentPage = page;
                UpdateTable();
                UpdatePagination();
                Debug.Log($"[MaterialInventoryPanel] Page: {currentPage}");
            }
        }
    }
}
