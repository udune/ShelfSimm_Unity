using System;
using UnityEngine;

namespace Data
{
    // 도서 정보를 나타내는 데이터 클래스
    [Serializable]
    public class BookData
    {
        [Header("기본 정보")]
        [SerializeField] private readonly string id; // 도서 고유 ID
        [SerializeField] private string title; // 도서 제목
        [SerializeField] private string author; // 저자
        [SerializeField] private string isbn; // ISBN 번호
        
        [Header("물리적 특성 (mm 단위)")]
        [SerializeField] private int thickness; // 두께 (mm)
        [SerializeField] private int height; // 높이 (mm)
        [SerializeField] private int width; // 너비 (mm)
        
        [Header("기타")]
        [SerializeField] private string category; // 카테고리
        [SerializeField] private bool isAvailable; // 사용 가능 여부
        
        // 생성자
        public BookData(string id, string title, string author, int thickness, int height, int width, string category = "일반", string isbn = "")
        {
            this.id = id; // 도서 고유 ID
            this.title = title; // 도서 제목
            this.author = author; // 저자
            this.thickness = thickness; // 두께 (mm)
            this.height = height; // 높이 (mm)
            this.width = width; // 너비 (mm)
            this.category = category; // 카테고리
            this.isbn = isbn; // ISBN 번호
            isAvailable = true; // 기본값: 사용 가능
        }

        // 기본 생성자
        public BookData()
        {
            isAvailable = true; // 기본값: 사용 가능
        }

        public string Id => id; // 도서 고유 ID
        public string Title => title; // 도서 제목
        public string Author => author; // 저자
        public string ISBN => isbn; // ISBN 번호
        public int Thickness => thickness; // 두께 (mm)
        public int Height => height; // 높이 (mm)
        public int Width => width; // 너비 (mm)
        public string Category => category; // 카테고리
        public bool IsAvailable => isAvailable; // 사용 가능 여부

        public string DisplayText => $"{title} - {author}"; // 드롭다운 표시용 문자열 (저자 포함)
        public string SimpleDisplayText => title; // 간단한 표시용 문자열 (제목만)
        public string DetailedInfo => $"{title} by {author} ({category}) - {thickness}mm x {height}mm x {width}mm"; // 상세 정보 문자열

        public void SetAvailability(bool available) // 사용 가능 여부 설정 메서드
        {
            isAvailable = available; // 사용 가능 여부 설정
        }

        public override string ToString() // 문자열 표현 재정의
        {
            return DisplayText; // 도서의 표시용 문자열 반환
        }

        // ID 기반 동등성 비교 재정의
        public override bool Equals(object obj)
        {
            if (obj is BookData other) // obj가 BookData 타입인지 확인
            {
                return id == other.id; // ID가 동일한지 비교
            }

            return false; // 타입이 다르면 false 반환
        }

        // ID 기반 해시 코드 생성 재정의
        public override int GetHashCode()
        {
            return id?.GetHashCode() ?? 0; // ID의 해시 코드 반환, null인 경우 0 반환
        }
    }
}
