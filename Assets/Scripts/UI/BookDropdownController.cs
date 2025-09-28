using System;
using System.Collections.Generic;
using Core;
using Data;
using TMPro;
using UnityEngine;

// 도서 선택 드롭다운 컨트롤러 클래스
public class BookDropdownController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown bookDropdown; // 도서 선택 드롭다운
    [SerializeField] private TextMeshProUGUI bookInfoText; // 도서 정보 텍스트
    
    [Header("References")]
    [SerializeField] private BookRegistry bookRegistry; // 도서 레지스트리 참조 (인스펙터에서 수동 할당 가능)
    
    [Header("Settings")]
    [SerializeField] private bool showDetailedInfo = false; // 상세 정보 표시 여부
    [SerializeField] private string emptySelectionText = "도서를 선택하세요"; // 빈 선택 시 표시 텍스트

    public Action<BookData> onBookSelected; // 도서 선택 시 콜백
    public Action<string> onBookIdSelected; // 도서 ID 선택 시 콜백
    
    private BookData[] availableBooks; // 사용 가능한 도서 배열
    private BookData selectedBook; // 현재 선택된 도서

    private void Start()
    {
        FindReferences(); // 참조 찾기
        InitDropdown(); // 드롭다운 초기화
        BindEvents(); // 이벤트 바인딩
    }

    private void FindReferences()
    {
        if (bookRegistry == null) // 수동 할당이 안 되었을 때
        {
            bookRegistry = FindObjectOfType<BookRegistry>(); // 씬에서 BookRegistry 컴포넌트 탐색
            if (bookRegistry == null) // 참조가 없으면 에러 로그 출력
            {
                Debug.LogError("Book Registry not found");
                return; // 초기화 중단
            }
        }

        if (bookDropdown == null) // 수동 할당이 안 되었을 때
        {
            bookDropdown = GetComponent<TMP_Dropdown>(); // 동일 게임 오브젝트에서 TMP_Dropdown 컴포넌트 탐색
            if (bookDropdown == null) // 참조가 없으면 에러 로그 출력
            {
                Debug.LogError("Book Dropdown not found");
                return; // 초기화 중단
            }
        }
        
        Debug.Log("[BookDropdownController] References found");
    }

    // 외부에서 초기화 및 이벤트 바인딩 호출
    private void InitDropdown()
    {
        if (bookRegistry == null || bookDropdown == null) // 참조가 없으면 초기화 중단
        {
            return; // 초기화 중단
        }

        availableBooks = bookRegistry.GetAllAvailableBooks(); // 사용 가능한 도서 배열 가져오기
        
        SetupDropdownOptions(); // 드롭다운 옵션 설정

        bookDropdown.value = 0; // 기본 선택값 설정 (빈 선택)
        selectedBook = null; // 선택된 도서 초기화
        
        Debug.Log($"[BookDropdownController] Dropdown initialized with {availableBooks.Length} books");
    }

    // 드롭다운 옵션 설정 메서드
    private void SetupDropdownOptions()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>(); // 옵션 리스트 초기화
        
        options.Add(new TMP_Dropdown.OptionData(emptySelectionText)); // 빈 선택 옵션 추가
        
        foreach (BookData book in availableBooks) // 사용 가능한 도서마다 옵션 추가
        {
            if (book != null) // 유효한 도서 데이터만 추가
            {
                string displayText = showDetailedInfo ? book.DetailedInfo : book.DisplayText; // 상세 정보 또는 간단한 텍스트
                options.Add(new TMP_Dropdown.OptionData(displayText)); // 옵션 추가
            }
        }

        bookDropdown.options = options; // 드롭다운 옵션 설정
        bookDropdown.RefreshShownValue(); // 표시 값 갱신
        
        Debug.Log("[BookDropdownController] Dropdown options set up");
    }

    // 외부에서 초기화 및 이벤트 바인딩 호출
    private void BindEvents()
    {
        if (bookDropdown != null) // 드롭다운 참조가 있을 때
        {
            bookDropdown.onValueChanged.AddListener(OnDropdownValueChanged); // 값 변경 이벤트에 리스너 추가
            Debug.Log("[BookDropdownController] Event bound");
        }
    }

    // 드롭다운 값 변경 시 호출되는 콜백 메서드
    private void OnDropdownValueChanged(int selectedIndex)
    {
        Debug.Log("[BookDropdownController] OnDropdownValueChanged");

        if (selectedIndex <= 0) // 빈 선택일 때
        {
            selectedBook = null; // 선택된 도서 초기화
            UpdateBookInfo(null); // 도서 정보 업데이트
            
            onBookSelected?.Invoke(null); // 도서 선택 콜백 호출
            onBookIdSelected?.Invoke(""); // 도서 ID 선택 콜백 호출
            return; // 빈 선택 처리 후 종료
        }
        
        int bookIndex = selectedIndex - 1; // 실제 도서 배열 인덱스 계산 (빈 선택 옵션 때문에 -1)

        if (bookIndex >= 0 && bookIndex < availableBooks.Length) // 유효한 인덱스일 때
        {
            selectedBook = availableBooks[bookIndex]; // 선택된 도서 설정
            UpdateBookInfo(selectedBook); // 도서 정보 업데이트
            
            onBookSelected?.Invoke(selectedBook); // 도서 선택 콜백 호출
            onBookIdSelected?.Invoke(selectedBook.Id); // 도서 ID 선택 콜백 호출
            
            Debug.Log($"[BookDropdownController] Selected book: {selectedBook.DisplayText}");
        }
        else
        {
            Debug.LogError($"[BookDropdownController] Invalid book index: {bookIndex}");
        }
    }

    // 도서 정보 텍스트 업데이트 메서드
    private void UpdateBookInfo(BookData book)
    {
        if (bookInfoText == null) // 도서 정보 텍스트 참조가 없으면 종료
        {
            return; // 종료
        }

        if (book == null) // 선택된 도서가 없을 때
        {
            bookInfoText.text = "선택된 도서가 없습니다."; // 기본 메시지
            bookInfoText.color = Color.gray; // 회색 텍스트
        }
        else
        {
            bookInfoText.text = showDetailedInfo ? book.DetailedInfo : 
                    $"선택된 도서: {book.DisplayText}\n크기: {book.Thickness}mm (두께) × {book.Height}mm (높이)"; // 도서 정보 표시
            bookInfoText.color = Color.black; // 검은색 텍스트
        }
    }
    
    public void SelectBookById(string bookId) // 도서 ID로 선택 메서드
    {
        if (string.IsNullOrEmpty(bookId)) // 빈 ID일 때
        {
            bookDropdown.value = 0; // 빈 선택으로 설정
            return; // 종료
        }

        for (int i = 0; i < availableBooks.Length; i++) // 도서 배열에서 ID 검색
        {
            if (availableBooks[i].Id == bookId) // ID가 일치할 때
            {
                bookDropdown.value = i + 1; // 드롭다운 인덱스 설정 (빈 선택 옵션 때문에 +1)
                return; // 종료
            }
        }
        
        Debug.LogWarning($"[BookDropdownController] Book ID not found: {bookId}");
    }
    
    public BookData GetSelectedBook() // 선택된 도서 반환 메서드
    {
        return selectedBook; // 선택된 도서 반환
    }
    
    public string GetSelectedBookId() // 선택된 도서 ID 반환 메서드
    {
        return selectedBook?.Id ?? ""; // 선택된 도서 ID 반환 (없으면 빈 문자열)
    }
    
    public bool HasSelectedBook() // 도서 선택 여부 확인 메서드
    {
        return selectedBook != null; // 도서가 선택되었는지 여부 반환
    }
    
    public void RefreshDropdown() // 드롭다운 갱신 메서드
    {
        if (bookRegistry != null) // 도서 레지스트리 참조가 있을 때
        {
            availableBooks = bookRegistry.GetAllAvailableBooks(); // 사용 가능한 도서 배열 갱신
            SetupDropdownOptions(); // 드롭다운 옵션 재설정
            bookDropdown.value = 0; // 기본 선택값 설정 (빈 선택)
            selectedBook = null; // 선택된 도서 초기화
            UpdateBookInfo(null); // 도서 정보 업데이트
            
            Debug.Log("[BookDropdownController] Dropdown refreshed");
        }
    }

    public void FilterByCategory(string category) // 카테고리별 필터링 메서드
    {
        if (bookRegistry == null) // 도서 레지스트리 참조가 없으면 종료
        {
            return; // 종료
        }
        
        BookData[] filteredBooks = string.IsNullOrEmpty(category) ?
            bookRegistry.GetAllAvailableBooks() :
            bookRegistry.GetBooksByCategory(category); // 카테고리별 도서 배열 가져오기

        availableBooks = filteredBooks; // 필터링된 도서 배열 설정
        SetupDropdownOptions(); // 드롭다운 옵션 재설정
        bookDropdown.value = 0; // 기본 선택값 설정 (빈 선택)
        selectedBook = null; // 선택된 도서 초기화
        UpdateBookInfo(null); // 도서 정보 업데이트
        
        Debug.Log($"[BookDropdownController] Filtered by category: {category}, {availableBooks.Length} books available");
    }

    private void OnDestroy() // 컴포넌트가 파괴될 때 이벤트 리스너 해제
    {
        if (bookDropdown != null) // 드롭다운 참조가 있을 때
        {
            bookDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged); // 값 변경 이벤트에서 리스너 제거
        }
    }

    public void TriggerSelectionEvent() // 외부에서 선택 이벤트 강제 트리거
    {
        OnDropdownValueChanged(bookDropdown.value); // 현재 선택된 값으로 이벤트 트리거
    }
}
