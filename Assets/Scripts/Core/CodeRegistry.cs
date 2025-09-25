using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    // 칸 코드를 관리하는 클래스
    public class CodeRegistry : MonoBehaviour
    {
        [Header("등록된 칸 코드 목록")]
        [SerializeField]
        private string[] registeredCodes = { "D20", "A15", "B03", "C10", "E05" }; // 초기 등록된 코드들

        private HashSet<string> codeSet; // 코드 저장용 해시셋

        public void Start()
        {
            Init(); // 초기화
        }

        // 코드 초기화 메서드
        private void Init()
        {
            codeSet = new HashSet<string>(); // 해시셋 초기화

            foreach (string code in registeredCodes) // 코드 등록
            {
                string normalizedCode = code.ToUpper().Trim(); // 대문자 변환 및 공백 제거
                codeSet.Add(normalizedCode); // 해시셋에 추가
                Debug.Log($"[CodeRegistry] 등록된 코드: {normalizedCode}");
            }
            
            Debug.Log($"[CodeRegistry] 총 {codeSet.Count}개 코드 등록 완료");
        }

        // 코드 유효성 검사 메서드
        public bool IsValidCode(string code)
        {
            if (string.IsNullOrEmpty(code)) // 빈 문자열 검사
            {
                return false; // 빈 문자열은 유효하지 않음
            }
            
            string normalizedCode = code.ToUpper().Trim(); // 대문자 변환 및 공백 제거
            return codeSet.Contains(normalizedCode); // 해시셋에 존재하는지 검사
        }

        // 모든 등록된 코드 반환 메서드
        public HashSet<string> GetAllCodes()
        {
            return new HashSet<string>(codeSet); // 해시셋 복사본 반환
        }

        // 새로운 코드 추가 메서드
        public void AddCode(string code)
        {
            if (!string.IsNullOrEmpty(code)) // 빈 문자열이 아닐 때만 추가
            {
                string normalizedCode = code.ToUpper().Trim(); // 대문자 변환 및 공백 제거
                codeSet.Add(normalizedCode); // 해시셋에 추가
                Debug.Log($"[CodeRegistry] 코드 추가됨: {normalizedCode}");
            }
        }
    }
}