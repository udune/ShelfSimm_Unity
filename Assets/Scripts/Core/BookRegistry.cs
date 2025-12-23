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
