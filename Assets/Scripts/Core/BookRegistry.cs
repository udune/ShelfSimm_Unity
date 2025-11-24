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
        [Header("더미 도서 데이터")]
        [SerializeField] private BookData[] dummyBooks =
        {
            new BookData("BOOK001", "C# 프로그래밍 입문", "홍길동", 30, 210, 148, "프로그래밍", "978-89-12345-67-8"),
            new BookData("BOOK002", "유니티 게임 개발", "김철수", 25, 200, 130, "게임 개발", "978-89-12345-68-5"),
            new BookData("BOOK003", "데이터 구조와 알고리즘", "이영희", 40, 220, 160, "컴퓨터 과학", "978-89-12345-69-2"),
            new BookData("BOOK004", "머신러닝 기초", "박민수", 35, 215, 155, "인공지능", "978-89-12345-70-8"),
            new BookData("BOOK005", "웹 개발 입문", "최지은", 28, 205, 140, "웹 개발", "978-89-12345-71-5")
        };

        private Dictionary<string, BookData> bookDatabase;
        private List<BookData> availableBooks;

        public void Awake()
        {
            InitDummyData();
            BuildDatabase();
        }

        private void InitDummyData()
        {
            if (dummyBooks == null || dummyBooks.Length == 0)
            {
                dummyBooks = CreateDefaultDummyBooks();
            }
        }
        
        private BookData[] CreateDefaultDummyBooks()
        {
            return new BookData[]
            {
                new BookData("BOOK001", "C# 프로그래밍 입문", "홍길동", 30, 210, 148, "프로그래밍", "978-89-12345-67-8"),
                new BookData("BOOK002", "유니티 게임 개발", "김철수", 25, 200, 130, "게임 개발", "978-89-12345-68-5"),
                new BookData("BOOK003", "데이터 구조와 알고리즘", "이영희", 40, 220, 160, "컴퓨터 과학", "978-89-12345-69-2"),
                new BookData("BOOK004", "머신러닝 기초", "박민수", 35, 215, 155, "인공지능", "978-89-12345-70-8"),
                new BookData("BOOK005", "웹 개발 입문", "최지은", 28, 205, 140, "웹 개발", "978-89-12345-71-5")
            };
        }

        private void BuildDatabase()
        {
            bookDatabase = new Dictionary<string, BookData>();
            availableBooks = new List<BookData>();

            foreach (BookData book in dummyBooks)
            {
                if (book != null && !string.IsNullOrEmpty(book.Id))
                {
                    bookDatabase[book.Id] = book;

                    if (book.IsAvailable)
                    {
                        availableBooks.Add(book);
                    }
                }
            }
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
            return availableBooks.ToArray();
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

        public BookData GetBookByIndex(int index)
        {
            if (index < 0 || index >= availableBooks.Count)
            {
                return null;
            }

            return availableBooks[index];
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
                    id: dto.id ?? $"BOOK_{Guid.NewGuid().ToString().Substring(0, 8)}",
                    title: dto.title ?? "Unknown Title",
                    author: "Unknown",
                    thickness: dto.thicknessMm,
                    height: dto.heightMm,
                    width: 150,
                    category: "일반",
                    isbn: ""
                );

                bookDatabase[bookData.Id] = bookData;
                availableBooks.Add(bookData);
            }
        }

        public void ResetToDummyData()
        {
            InitDummyData();
            BuildDatabase();
        }
    }
}
