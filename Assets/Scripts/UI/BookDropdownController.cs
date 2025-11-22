using System;
using System.Collections.Generic;
using Core;
using Data;
using TMPro;
using UnityEngine;

namespace UI
{
    public class BookDropdownController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Dropdown bookDropdown;
        [SerializeField] private TextMeshProUGUI bookInfoText;

        [Header("References")]
        [SerializeField] private BookRegistry bookRegistry;

        [Header("Settings")]
        [SerializeField] private bool showDetailedInfo = false;
        [SerializeField] private string emptySelectionText = "도서를 선택하세요";

        public Action<BookData> onBookSelected;
        public Action<string> onBookIdSelected;

        private BookData[] availableBooks;
        private BookData selectedBook;

        private void Start()
        {
            FindReferences();
            InitDropdown();
            BindEvents();
        }

        private void FindReferences()
        {
            if (bookRegistry == null)
            {
                bookRegistry = FindObjectOfType<BookRegistry>();
                if (bookRegistry == null)
                {
                    Debug.LogError("Book Registry not found");
                    return;
                }
            }

            if (bookDropdown == null)
            {
                bookDropdown = GetComponent<TMP_Dropdown>();
                if (bookDropdown == null)
                {
                    Debug.LogError("Book Dropdown not found");
                    return;
                }
            }
        }

        private void InitDropdown()
        {
            if (bookRegistry == null || bookDropdown == null)
            {
                return;
            }

            availableBooks = bookRegistry.GetAllAvailableBooks();
            SetupDropdownOptions();
            bookDropdown.value = 0;
            selectedBook = null;
        }

        private void SetupDropdownOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData(emptySelectionText));

            foreach (BookData book in availableBooks)
            {
                if (book != null)
                {
                    string displayText = showDetailedInfo ? book.DetailedInfo : book.DisplayText;
                    options.Add(new TMP_Dropdown.OptionData(displayText));
                }
            }

            bookDropdown.options = options;
            bookDropdown.RefreshShownValue();
        }

        private void BindEvents()
        {
            if (bookDropdown != null)
            {
                bookDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        private void OnDropdownValueChanged(int selectedIndex)
        {
            if (selectedIndex <= 0)
            {
                selectedBook = null;
                UpdateBookInfo(null);
                onBookSelected?.Invoke(null);
                onBookIdSelected?.Invoke("");
                return;
            }

            int bookIndex = selectedIndex - 1;

            if (bookIndex >= 0 && bookIndex < availableBooks.Length)
            {
                selectedBook = availableBooks[bookIndex];
                UpdateBookInfo(selectedBook);
                onBookSelected?.Invoke(selectedBook);
                onBookIdSelected?.Invoke(selectedBook.Id);
            }
            else
            {
                Debug.LogError($"[BookDropdownController] Invalid book index: {bookIndex}");
            }
        }

        private void UpdateBookInfo(BookData book)
        {
            if (bookInfoText == null)
            {
                return;
            }

            if (book == null)
            {
                bookInfoText.text = "선택된 도서가 없습니다.";
                bookInfoText.color = Color.gray;
            }
            else
            {
                bookInfoText.text = showDetailedInfo ? book.DetailedInfo :
                    $"선택된 도서: {book.DisplayText}\n크기: {book.Thickness}mm (두께) × {book.Height}mm (높이)";
                bookInfoText.color = Color.black;
            }
        }

        public void SelectBookById(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                bookDropdown.value = 0;
                return;
            }

            for (int i = 0; i < availableBooks.Length; i++)
            {
                if (availableBooks[i].Id == bookId)
                {
                    bookDropdown.value = i + 1;
                    return;
                }
            }

            Debug.LogWarning($"[BookDropdownController] Book ID not found: {bookId}");
        }

        public BookData GetSelectedBook()
        {
            return selectedBook;
        }

        public string GetSelectedBookId()
        {
            return selectedBook?.Id ?? "";
        }

        public bool HasSelectedBook()
        {
            return selectedBook != null;
        }

        public void RefreshDropdown()
        {
            if (bookRegistry != null)
            {
                availableBooks = bookRegistry.GetAllAvailableBooks();
                SetupDropdownOptions();
                bookDropdown.value = 0;
                selectedBook = null;
                UpdateBookInfo(null);
            }
        }

        public void FilterByCategory(string category)
        {
            if (bookRegistry == null)
            {
                return;
            }

            BookData[] filteredBooks = string.IsNullOrEmpty(category) ?
                bookRegistry.GetAllAvailableBooks() :
                bookRegistry.GetBooksByCategory(category);

            availableBooks = filteredBooks;
            SetupDropdownOptions();
            bookDropdown.value = 0;
            selectedBook = null;
            UpdateBookInfo(null);
        }

        private void OnDestroy()
        {
            if (bookDropdown != null)
            {
                bookDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }
        }

        public void TriggerSelectionEvent()
        {
            OnDropdownValueChanged(bookDropdown.value);
        }
    }
}
