using System;
using NUnit.Framework;
using UnityEngine;
using Core;
using Data;
using Object = UnityEngine.Object;

namespace Tests.EditMode
{
    /// <summary>
    /// T-105: 도서 드롭다운 바인딩 (더미데이터) 테스트
    /// </summary>
    public class BookRegistryTests
    {
        private GameObject testObject;
        private BookRegistry bookRegistry;

        [SetUp]
        public void Setup()
        {
            testObject = new GameObject("TestBookRegistry");
            bookRegistry = testObject.AddComponent<BookRegistry>();
            
            // Start() 메서드 호출하여 초기화
            bookRegistry.Start();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region 기본 기능 테스트

        [Test]
        public void BookRegistry_Initialization_CreatesDefaultBooks()
        {
            // When: BookRegistry가 초기화됨
            // Then: 기본 더미 도서들이 생성되어야 함
            var allBooks = bookRegistry.GetAllAvailableBooks();
            
            Assert.IsNotNull(allBooks, "도서 목록이 null이어서는 안됨");
            Assert.Greater(allBooks.Length, 0, "최소 1개 이상의 도서가 있어야 함");
            
            Debug.Log($"생성된 도서 개수: {allBooks.Length}");
        }

        [Test]
        public void BookRegistry_GetBookById_ValidId_ReturnsCorrectBook()
        {
            // Given: 유효한 도서 ID
            var allBooks = bookRegistry.GetAllAvailableBooks();
            Assert.Greater(allBooks.Length, 0, "테스트를 위해 도서가 필요함");
            
            string testBookId = allBooks[0].Id;
            
            // When: ID로 도서 검색
            var foundBook = bookRegistry.GetBookById(testBookId);
            
            // Then: 올바른 도서가 반환되어야 함
            Assert.IsNotNull(foundBook, "도서를 찾을 수 있어야 함");
            Assert.AreEqual(testBookId, foundBook.Id, "ID가 일치해야 함");
        }

        [Test]
        public void BookRegistry_GetBookById_InvalidId_ReturnsNull()
        {
            // Given: 존재하지 않는 도서 ID
            string invalidId = "INVALID_BOOK_ID";
            
            // When: ID로 도서 검색
            var foundBook = bookRegistry.GetBookById(invalidId);
            
            // Then: null이 반환되어야 함
            Assert.IsNull(foundBook, "존재하지 않는 ID로는 null이 반환되어야 함");
        }

        [Test]
        public void BookRegistry_GetBookById_NullOrEmpty_ReturnsNull()
        {
            // Given: null이나 빈 문자열 ID
            string[] invalidIds = { null, "", "   " };
            
            foreach (string invalidId in invalidIds)
            {
                // When: 무효한 ID로 도서 검색
                var foundBook = bookRegistry.GetBookById(invalidId);
                
                // Then: null이 반환되어야 함
                Assert.IsNull(foundBook, $"무효한 ID '{invalidId}'로는 null이 반환되어야 함");
            }
        }

        #endregion

        #region 도서 목록 및 검색 테스트

        [Test]
        public void BookRegistry_GetAllAvailableBooks_ReturnsNonEmptyArray()
        {
            // When: 모든 사용 가능한 도서 조회
            var books = bookRegistry.GetAllAvailableBooks();
            
            // Then: 비어있지 않은 배열이 반환되어야 함
            Assert.IsNotNull(books, "도서 배열이 null이어서는 안됨");
            Assert.Greater(books.Length, 0, "최소 1개 이상의 도서가 있어야 함");
            
            // 모든 도서가 사용 가능 상태여야 함
            foreach (var book in books)
            {
                Assert.IsTrue(book.IsAvailable, $"도서 '{book.Title}'은 사용 가능해야 함");
            }
        }

        [Test]
        public void BookRegistry_GetBooksByCategory_ValidCategory_ReturnsFilteredBooks()
        {
            // Given: 유효한 카테고리
            string category = "프로그래밍";
            
            // When: 카테고리별 도서 검색
            var books = bookRegistry.GetBooksByCategory(category);
            
            // Then: 해당 카테고리의 도서들만 반환되어야 함
            Assert.IsNotNull(books, "도서 배열이 null이어서는 안됨");
            
            foreach (var book in books)
            {
                Assert.AreEqual(category, book.Category, $"도서 '{book.Title}'의 카테고리가 일치해야 함");
            }
        }

        [Test]
        public void BookRegistry_SearchBooks_ValidSearchTerm_ReturnsMatchingBooks()
        {
            // Given: 검색어
            string searchTerm = "Clean"; // "Clean Code" 책을 찾기 위한 검색어
            
            // When: 도서 검색
            var books = bookRegistry.SearchBooks(searchTerm);
            
            // Then: 검색어와 일치하는 도서들이 반환되어야 함
            Assert.IsNotNull(books, "검색 결과가 null이어서는 안됨");
            
            foreach (var book in books)
            {
                bool matchFound = book.Title.ToLower().Contains(searchTerm.ToLower()) ||
                                book.Author.ToLower().Contains(searchTerm.ToLower()) ||
                                book.Category.ToLower().Contains(searchTerm.ToLower());
                Assert.IsTrue(matchFound, $"도서 '{book.Title}'에서 검색어 '{searchTerm}'를 찾을 수 있어야 함");
            }
        }

        [Test]
        public void BookRegistry_SearchBooks_EmptySearchTerm_ReturnsAllBooks()
        {
            // Given: 빈 검색어
            string[] emptySearchTerms = { "", null, "   " };
            
            foreach (string searchTerm in emptySearchTerms)
            {
                // When: 빈 검색어로 검색
                var searchResults = bookRegistry.SearchBooks(searchTerm);
                var allBooks = bookRegistry.GetAllAvailableBooks();
                
                // Then: 모든 도서가 반환되어야 함
                Assert.AreEqual(allBooks.Length, searchResults.Length, 
                    $"빈 검색어 '{searchTerm}'로는 모든 도서가 반환되어야 함");
            }
        }

        #endregion

        #region 드롭다운 연동 테스트

        [Test]
        public void BookRegistry_GetBookDisplayTexts_ReturnsCorrectFormat()
        {
            // When: 드롭다운용 표시 텍스트 조회
            var displayTexts = bookRegistry.GetBookDisplayTexts();
            var books = bookRegistry.GetAllAvailableBooks();
            
            // Then: 도서 개수와 일치하고 올바른 형식이어야 함
            Assert.AreEqual(books.Length, displayTexts.Length, "표시 텍스트 개수가 도서 개수와 일치해야 함");
            
            for (int i = 0; i < books.Length; i++)
            {
                string expectedText = books[i].DisplayText; // "제목 - 저자" 형식
                Assert.AreEqual(expectedText, displayTexts[i], $"인덱스 {i}의 표시 텍스트가 일치해야 함");
                Assert.IsTrue(displayTexts[i].Contains(" - "), "표시 텍스트에 ' - ' 구분자가 포함되어야 함");
            }
        }

        [Test]
        public void BookRegistry_GetBookIds_ReturnsCorrectIds()
        {
            // When: 드롭다운용 ID 배열 조회
            var bookIds = bookRegistry.GetBookIds();
            var books = bookRegistry.GetAllAvailableBooks();
            
            // Then: 도서 개수와 일치하고 올바른 ID들이어야 함
            Assert.AreEqual(books.Length, bookIds.Length, "ID 개수가 도서 개수와 일치해야 함");
            
            for (int i = 0; i < books.Length; i++)
            {
                Assert.AreEqual(books[i].Id, bookIds[i], $"인덱스 {i}의 ID가 일치해야 함");
                Assert.IsFalse(string.IsNullOrEmpty(bookIds[i]), "ID가 비어있어서는 안됨");
            }
        }

        [Test]
        public void BookRegistry_GetBookByIndex_ValidIndex_ReturnsCorrectBook()
        {
            // Given: 유효한 인덱스
            var books = bookRegistry.GetAllAvailableBooks();
            Assert.Greater(books.Length, 0, "테스트를 위해 도서가 필요함");
            
            int testIndex = 0;
            
            // When: 인덱스로 도서 조회
            var book = bookRegistry.GetBookByIndex(testIndex);
            
            // Then: 올바른 도서가 반환되어야 함
            Assert.IsNotNull(book, "유효한 인덱스로는 도서가 반환되어야 함");
            Assert.AreEqual(books[testIndex].Id, book.Id, "인덱스에 해당하는 도서가 반환되어야 함");
        }

        [Test]
        public void BookRegistry_GetBookByIndex_InvalidIndex_ReturnsNull()
        {
            // Given: 무효한 인덱스들
            int[] invalidIndices = { -1, 1000 };
            
            foreach (int index in invalidIndices)
            {
                // When: 무효한 인덱스로 도서 조회
                var book = bookRegistry.GetBookByIndex(index);
                
                // Then: null이 반환되어야 함
                Assert.IsNull(book, $"무효한 인덱스 {index}로는 null이 반환되어야 함");
            }
        }

        #endregion

        #region 카테고리 및 기타 기능 테스트

        [Test]
        public void BookRegistry_GetAllCategories_ReturnsUniqueCategories()
        {
            // When: 모든 카테고리 조회
            var categories = bookRegistry.GetAllCategories();
            
            // Then: 중복 없는 카테고리 목록이 반환되어야 함
            Assert.IsNotNull(categories, "카테고리 배열이 null이어서는 안됨");
            Assert.Greater(categories.Length, 0, "최소 1개 이상의 카테고리가 있어야 함");
            
            // 중복 확인
            var uniqueCategories = new System.Collections.Generic.HashSet<string>(categories);
            Assert.AreEqual(uniqueCategories.Count, categories.Length, "카테고리에 중복이 없어야 함");
            
            // 정렬 확인
            for (int i = 1; i < categories.Length; i++)
            {
                Assert.LessOrEqual(String.CompareOrdinal(categories[i-1], categories[i]), 0, "카테고리가 알파벳순으로 정렬되어야 함");
            }
        }

        [Test]
        public void BookRegistry_GetBookCount_ReturnsCorrectCount()
        {
            // When: 도서 개수 조회
            int count = bookRegistry.GetBookCount();
            var books = bookRegistry.GetAllAvailableBooks();
            
            // Then: 실제 도서 개수와 일치해야 함
            Assert.AreEqual(books.Length, count, "반환된 개수가 실제 도서 개수와 일치해야 함");
            Assert.Greater(count, 0, "최소 1개 이상의 도서가 있어야 함");
        }

        #endregion

        #region 동적 추가/제거 테스트

        [Test]
        public void BookRegistry_AddBook_ValidBook_AddsSuccessfully()
        {
            // Given: 새로운 도서
            var newBook = new BookData("TEST001", "Test Book", "Test Author", 25, 200, 150, "테스트");
            int initialCount = bookRegistry.GetBookCount();
            
            // When: 도서 추가
            bookRegistry.AddBook(newBook);
            
            // Then: 도서가 추가되어야 함
            Assert.AreEqual(initialCount + 1, bookRegistry.GetBookCount(), "도서 개수가 1 증가해야 함");
            
            var addedBook = bookRegistry.GetBookById("TEST001");
            Assert.IsNotNull(addedBook, "추가된 도서를 찾을 수 있어야 함");
            Assert.AreEqual("Test Book", addedBook.Title, "제목이 일치해야 함");
        }

        [Test]
        public void BookRegistry_AddBook_DuplicateId_DoesNotAdd()
        {
            // Given: 기존 도서와 동일한 ID를 가진 새 도서
            var existingBooks = bookRegistry.GetAllAvailableBooks();
            Assert.Greater(existingBooks.Length, 0, "테스트를 위해 기존 도서가 필요함");
            
            string duplicateId = existingBooks[0].Id;
            var duplicateBook = new BookData(duplicateId, "Duplicate Book", "Test Author", 25, 200, 150);
            int initialCount = bookRegistry.GetBookCount();
            
            // When: 중복 ID로 도서 추가 시도
            bookRegistry.AddBook(duplicateBook);
            
            // Then: 도서가 추가되지 않아야 함
            Assert.AreEqual(initialCount, bookRegistry.GetBookCount(), "중복 ID로는 도서가 추가되지 않아야 함");
        }

        [Test]
        public void BookRegistry_RemoveBook_ValidId_RemovesSuccessfully()
        {
            // Given: 기존 도서
            var books = bookRegistry.GetAllAvailableBooks();
            Assert.Greater(books.Length, 0, "테스트를 위해 도서가 필요함");
            
            string bookIdToRemove = books[0].Id;
            int initialCount = bookRegistry.GetBookCount();
            
            // When: 도서 제거
            bool result = bookRegistry.RemoveBook(bookIdToRemove);
            
            // Then: 도서가 제거되어야 함
            Assert.IsTrue(result, "제거 작업이 성공해야 함");
            Assert.AreEqual(initialCount - 1, bookRegistry.GetBookCount(), "도서 개수가 1 감소해야 함");
            
            var removedBook = bookRegistry.GetBookById(bookIdToRemove);
            Assert.IsNull(removedBook, "제거된 도서는 찾을 수 없어야 함");
        }

        [Test]
        public void BookRegistry_RemoveBook_InvalidId_ReturnsFalse()
        {
            // Given: 존재하지 않는 도서 ID
            string invalidId = "INVALID_BOOK_ID";
            int initialCount = bookRegistry.GetBookCount();
            
            // When: 존재하지 않는 도서 제거 시도
            bool result = bookRegistry.RemoveBook(invalidId);
            
            // Then: 제거 실패해야 함
            Assert.IsFalse(result, "존재하지 않는 ID로는 제거가 실패해야 함");
            Assert.AreEqual(initialCount, bookRegistry.GetBookCount(), "도서 개수가 변경되지 않아야 함");
        }

        #endregion

        #region T-105 태스크 특화 테스트

        [Test]
        [Description("T-105: 더미 도서 데이터가 올바르게 생성되는지 확인")]
        public void T105_DummyBookData_CreatedWithCorrectStructure()
        {
            // When: 더미 도서 데이터 조회
            var books = bookRegistry.GetAllAvailableBooks();
            
            // Then: 기본 더미 도서들이 올바른 구조로 생성되어야 함
            Assert.GreaterOrEqual(books.Length, 5, "최소 5개 이상의 더미 도서가 있어야 함");
            
            foreach (var book in books)
            {
                // 필수 필드 검증
                Assert.IsFalse(string.IsNullOrEmpty(book.Id), "도서 ID가 있어야 함");
                Assert.IsFalse(string.IsNullOrEmpty(book.Title), "도서 제목이 있어야 함");
                Assert.IsFalse(string.IsNullOrEmpty(book.Author), "저자가 있어야 함");
                Assert.IsFalse(string.IsNullOrEmpty(book.Category), "카테고리가 있어야 함");
                
                // 물리적 특성 검증
                Assert.Greater(book.Thickness, 0, "두께가 0보다 커야 함");
                Assert.Greater(book.Height, 0, "높이가 0보다 커야 함");
                Assert.Greater(book.Width, 0, "너비가 0보다 커야 함");
                
                // 사용 가능 상태 검증
                Assert.IsTrue(book.IsAvailable, "더미 도서는 모두 사용 가능해야 함");
            }
        }

        [Test]
        [Description("T-105: 드롭다운 바인딩용 데이터 형식이 올바른지 확인")]
        public void T105_DropdownBinding_DataFormatIsCorrect()
        {
            // When: 드롭다운 바인딩용 데이터 조회
            var displayTexts = bookRegistry.GetBookDisplayTexts();
            var bookIds = bookRegistry.GetBookIds();
            
            // Then: 올바른 형식이어야 함
            Assert.AreEqual(displayTexts.Length, bookIds.Length, "표시 텍스트와 ID 개수가 일치해야 함");
            
            for (int i = 0; i < displayTexts.Length; i++)
            {
                // 표시 텍스트 형식 검증: "제목 - 저자"
                Assert.IsTrue(displayTexts[i].Contains(" - "), $"표시 텍스트 '{displayTexts[i]}'에 ' - ' 구분자가 있어야 함");
                
                // ID 형식 검증
                Assert.IsFalse(string.IsNullOrEmpty(bookIds[i]), $"인덱스 {i}의 ID가 비어있어서는 안됨");
                Assert.IsTrue(bookIds[i].StartsWith("BOOK"), $"ID '{bookIds[i]}'는 'BOOK'으로 시작해야 함");
            }
        }

        #endregion
    }
}