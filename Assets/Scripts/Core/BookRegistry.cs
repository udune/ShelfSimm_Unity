using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using API;

namespace Core
{
    // 도서 데이터를 관리하는 클래스
    public class BookRegistry : MonoBehaviour
    {
        public static BookRegistry Instance { get; private set; }

        private Dictionary<string, BookData> bookDatabase = new Dictionary<string, BookData>();
        private List<BookData> availableBooks = new List<BookData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public BookData GetBookById(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
            {
                return null;
            }

            bookDatabase.TryGetValue(bookId, out BookData book);
            return book;
        }

        public BookData[] GetAllAvailableBooks()
        {
            return availableBooks?.ToArray();
        }

        public BookData[] GetBooksByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return GetAllAvailableBooks();
            }

            return availableBooks
                .Where(book => book.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        public BookData[] SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return GetAllAvailableBooks();
            }

            string lowerSearchTerm = searchTerm.ToLower();

            return availableBooks
                .Where(book => book.Title.ToLower().Contains(lowerSearchTerm) ||
                               book.Author.ToLower().Contains(lowerSearchTerm) ||
                               book.Category.ToLower().Contains(lowerSearchTerm))
                .ToArray();
        }

        public string[] GetBookDisplayTexts()
        {
            return availableBooks.Select(book => book.DisplayText).ToArray();
        }

        public string[] GetBookIds()
        {
            return availableBooks.Select(book => book.Id).ToArray();
        }

        public string[] GetAllCategories()
        {
            return availableBooks
                .Select(book => book.Category)
                .Distinct()
                .OrderBy(category => category)
                .ToArray();
        }

        public int GetBookCount()
        {
            return bookDatabase.Count;
        }

        public BookData GetBookByTitle(string title)
        {
            return availableBooks.Find(x => x.Title.Equals(title));
        }

        public BookData GetBookByIndex(int index)
        {
            if (index < 0 || index >= availableBooks.Count)
            {
                return null;
            }

            return availableBooks[index];
        }

        public BookData GetDefaultBook()
        {
            if (availableBooks != null && availableBooks.Count > 0)
            {
                return availableBooks[0];
            }
            return null;
        }

        public void AddBook(BookData newBook)
        {
            if (newBook == null || string.IsNullOrEmpty(newBook.Id))
            {
                Debug.LogError("[BookRegistry] 유효하지 않은 도서 데이터입니다");
                return;
            }

            if (bookDatabase.ContainsKey(newBook.Id))
            {
                Debug.LogWarning($"[BookRegistry] 이미 존재하는 도서 ID입니다: {newBook.Id}");
                return;
            }

            bookDatabase[newBook.Id] = newBook;

            if (newBook.IsAvailable)
            {
                availableBooks.Add(newBook);
            }
        }

        public bool RemoveBook(string bookId)
        {
            if (string.IsNullOrEmpty(bookId) || !bookDatabase.ContainsKey(bookId))
            {
                return false;
            }

            BookData book = bookDatabase[bookId];
            bookDatabase.Remove(bookId);
            availableBooks.Remove(book);
            return true;
        }

        public void LoadBooksFromApi(List<BookDto> bookDtos)
        {
            if (bookDtos == null || bookDtos.Count == 0)
            {
                return;
            }

            bookDatabase.Clear();
            availableBooks.Clear();

            foreach (var dto in bookDtos)
            {
                var bookData = new BookData(
                    id: $"BOOK_{dto.id}",
                    title: dto.title,
                    author: dto.author,
                    thickness: dto.thicknessMn,
                    height: dto.heightMm,
                    width: 150,
                    stock: dto.stockQuantity,
                    category: "일반",
                    isbn: dto.sku
                );

                if (bookDatabase != null)
                {
                    bookDatabase[bookData.Id] = bookData;
                }

                if (availableBooks != null)
                {
                    availableBooks.Add(bookData);
                }
            }
        }
    }
}
