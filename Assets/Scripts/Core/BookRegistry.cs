using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

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
        }; // 더미 도서 데이터 배열

        private Dictionary<string, BookData> bookDatabase; // ID 기반 빠른 검색용
        private List<BookData> availableBooks; // 사용 가능한 도서 목록

        public void Start()
        {
            InitDummyData(); // 더미 데이터 초기화
            BuildDatabase(); // 데이터베이스 구축
        }

        private void InitDummyData()
        {
            if (dummyBooks == null || dummyBooks.Length == 0) // 더미 데이터가 없으면 기본 데이터 생성
            {
                dummyBooks = CreateDefaultDummyBooks(); // 기본 더미 도서 데이터 생성
                Debug.Log("[BookRegistry] 기본 더미 도서 데이터 생성됨");
            }
            
            Debug.Log($"[BookRegistry] 총 {dummyBooks.Length}개 도서 로드됨");
        }
        
        private BookData[] CreateDefaultDummyBooks() // 기본 더미 도서 데이터 생성
        {
            return new BookData[] // 기본 더미 도서 데이터 배열
            {
                new BookData("BOOK001", "C# 프로그래밍 입문", "홍길동", 30, 210, 148, "프로그래밍", "978-89-12345-67-8"),
                new BookData("BOOK002", "유니티 게임 개발", "김철수", 25, 200, 130, "게임 개발", "978-89-12345-68-5"),
                new BookData("BOOK003", "데이터 구조와 알고리즘", "이영희", 40, 220, 160, "컴퓨터 과학", "978-89-12345-69-2"),
                new BookData("BOOK004", "머신러닝 기초", "박민수", 35, 215, 155, "인공지능", "978-89-12345-70-8"),
                new BookData("BOOK005", "웹 개발 입문", "최지은", 28, 205, 140, "웹 개발", "978-89-12345-71-5")
            };
        }

        // 데이터베이스 구축 (검색 최적화)
        private void BuildDatabase()
        {
            bookDatabase = new Dictionary<string, BookData>(); // ID 기반 빠른 검색용 딕셔너리 초기화
            availableBooks = new List<BookData>(); // 사용 가능한 도서 목록 초기화

            foreach (BookData book in dummyBooks) // 더미 도서 데이터 순회
            {
                if (book != null && !string.IsNullOrEmpty(book.Id)) // 유효한 도서 데이터만 추가
                {
                    bookDatabase[book.Id] = book; // ID 기반 딕셔너리에 추가

                    if (book.IsAvailable) // 사용 가능한 도서만 목록에 추가
                    {
                        availableBooks.Add(book); // 사용 가능한 도서 목록에 추가
                    }
                }
            }
            
            Debug.Log($"[BookRegistry] 데이터베이스 구축 완료: {bookDatabase.Count}개 도서, {availableBooks.Count}개 사용 가능");
        }

        // ID로 도서 찾기
        public BookData GetBookById(string bookId)
        {
            if (string.IsNullOrEmpty(bookId)) // 빈 문자열 검사
            {
                return null; // 유효하지 않은 ID인 경우 null 반환
            }

            bookDatabase.TryGetValue(bookId, out BookData book); // 딕셔너리에서 도서 검색
            return book; // 도서 반환 (없으면 null)
        }

        // 모든 사용 가능한 도서 반환
        public BookData[] GetAllAvailableBooks()
        {
            return availableBooks.ToArray(); // 사용 가능한 도서 목록을 배열로 반환
        }

        // 카테고리별 도서 반환
        public BookData[] GetBooksByCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) // 빈 문자열 검사
            {
                return GetAllAvailableBooks(); // 빈 문자열인 경우 모든 사용 가능한 도서 반환
            }

            return availableBooks // 카테고리로 필터링
                .Where(book => book.Category.Equals(category, StringComparison.OrdinalIgnoreCase)) // 대소문자 구분 없이 비교
                .ToArray(); // 필터링된 도서 배열 반환
        }

        // 검색어로 도서 찾기 (제목, 저자, 카테고리에서 검색)
        public BookData[] SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) // 빈 문자열 및 공백 문자열 검사
            {
                return GetAllAvailableBooks(); // 빈 문자열인 경우 모든 사용 가능한 도서 반환
            }
            
            string lowerSearchTerm = searchTerm.ToLower(); // 소문자로 변환하여 대소문자 구분 없이 검색

            return availableBooks // 제목, 저자, 카테고리에서 검색
                .Where(book => book.Title.ToLower().Contains(lowerSearchTerm) ||
                               book.Author.ToLower().Contains(lowerSearchTerm) ||
                               book.Category.ToLower().Contains(lowerSearchTerm)) // 검색어 포함 여부 검사
                .ToArray(); // 필터링된 도서 배열 반환
        }
        
        public string[] GetBookDisplayTexts() // 도서의 표시용 문자열 배열 반환
        {
            return availableBooks.Select(book => book.DisplayText).ToArray(); // 도서의 표시용 문자열 배열 반환
        }

        public string[] GetBookIds() // 도서의 ID 배열 반환 (인덱스 매칭용)
        {
            return availableBooks.Select(book => book.Id).ToArray(); // 도서의 ID 배열 반환
        }

        public string[] GetAllCategories() // 모든 카테고리 반환 (중복 제거 및 정렬)
        {
            return availableBooks // 카테고리 추출, 중복 제거, 정렬
                .Select(book => book.Category) // 도서의 카테고리 선택
                .Distinct() // 중복 제거
                .OrderBy(category => category) // 알파벳 순으로 정렬
                .ToArray(); // 카테고리 배열 반환
        }
        
        public int GetBookCount() // 전체 도서 수 반환
        {
            return bookDatabase.Count; // 전체 도서 수 반환
        }

        public BookData GetBookByIndex(int index) // 특정 인덱스의 도서 반환 (드롭다운 인덱스 기반)
        {
            if (index < 0 || index >= availableBooks.Count) // 인덱스 범위 검사
            {
                return null; // 범위를 벗어난 경우 null 반환
            }
            
            return availableBooks[index]; // 해당 인덱스의 도서 반환
        }

        public void AddBook(BookData newBook) // 도서 추가 (런타임에서 동적 추가용)
        {
            if (newBook == null || string.IsNullOrEmpty(newBook.Id)) // 유효성 검사
            {
                Debug.LogError("[BookRegistry] 유효하지 않은 도서 데이터입니다");
                return; // 유효하지 않은 도서 데이터인 경우 종료
            }

            if (bookDatabase.ContainsKey(newBook.Id)) // 이미 존재하는 ID인지 검사
            {
                Debug.LogWarning($"[BookRegistry] 이미 존재하는 도서 ID입니다: {newBook.Id}");
                return; // 이미 존재하는 도서 ID인 경우 종료
            }

            bookDatabase[newBook.Id] = newBook; // 딕셔너리에 추가

            if (newBook.IsAvailable) // 사용 가능한 도서인 경우 목록에 추가
            {
                availableBooks.Add(newBook); // 사용 가능한 도서 목록에 추가
            }
            
            Debug.Log($"[BookRegistry] 도서 추가됨: {newBook.DisplayText}");
        }

        public bool RemoveBook(string bookId) // 도서 제거
        {
            if (string.IsNullOrEmpty(bookId) || !bookDatabase.ContainsKey(bookId)) // 유효성 검사
            {
                return false; // 유효하지 않은 ID인 경우 false 반환
            }
            
            BookData book = bookDatabase[bookId]; // 제거할 도서 가져오기
            bookDatabase.Remove(bookId); // 딕셔너리에서 제거
            availableBooks.Remove(book); // 사용 가능한 도서 목록에서 제거
            
            Debug.Log($"[BookRegistry] 도서 제거됨: {book.DisplayText}");
            return true; // 성공적으로 제거된 경우 true 반환
        }
    }
}
