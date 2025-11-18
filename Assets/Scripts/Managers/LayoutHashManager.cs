using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Core;
using Data;
using UnityEngine;

namespace Managers
{
    // LayoutHashManager는 CellsLayoutSO의 레이아웃 정보를 기반으로 고유한 해시 값을 생성하고 관리하는 클래스입니다.
    public class LayoutHashManager : MonoBehaviour
    {
        [SerializeField] private PathCache pathCache; // PathCache 참조 (옵션)

        private string lastComputedHash = ""; // 마지막으로 계산된 해시 값 저장

        public void UpdateLayoutHash(CellsLayoutSO layout) // 레이아웃 해시 값을 업데이트하는 메서드
        {
            if (layout == null) // 레이아웃이 null인지 확인
            {
                Debug.LogWarning("[LayoutHashManager] Layout is null, cannot compute hash.");
                return; // null이면 경고 메시지 출력 후 종료
            }

            string newHash = ComputeLayoutHash(layout); // 새로운 해시 값 계산
            
            if (string.IsNullOrEmpty(layout.layout_hash) || layout.layout_hash != newHash) // 해시 값이 비어있거나 변경되었는지 확인
            {
                layout.layout_hash = newHash; // 레이아웃의 해시 값 업데이트
                #if UNITY_EDITOR // 유니티 에디터에서만 실행
                UnityEditor.EditorUtility.SetDirty(layout); // 레이아웃 오브젝트를 더티 상태로 표시 (변경 사항 저장)
                #endif
            }

            if (pathCache != null) // PathCache가 설정되어 있으면
            {
                pathCache.SetLayoutHash(newHash); // PathCache에 새로운 해시 값 설정
            }
            
            lastComputedHash = newHash; // 마지막으로 계산된 해시 값 저장
            
            Debug.Log($"[LayoutHashManager] Layout hash updated to: {newHash}");
        }

        private string ComputeLayoutHash(CellsLayoutSO layout) // 레이아웃 정보를 기반으로 해시 값을 계산하는 메서드
        {
            StringBuilder sb = new StringBuilder(); // 문자열 빌더 생성
            sb.Append($"{layout.schema_version}"); // 스키마 버전 추가
            sb.Append($"{layout.grid_size.x},{layout.grid_size.y};"); // 그리드 크기 추가
            sb.Append($"{layout.warehouse.x},{layout.warehouse.y};"); // 창고 위치 추가
            
            var sortedCells = new List<CellDef>(layout.cells); // 셀 목록 복사
            sortedCells.Sort((a, b) => string.Compare(a.code, b.code, System.StringComparison.Ordinal)); // 셀 코드를 기준으로 정렬

            foreach (var cell in sortedCells) // 각 셀에 대해
            {
                sb.Append($"{cell.code},{cell.x},{cell.y};"); // 셀 코드와 위치 추가
                sb.Append($"{cell.width},{cell.height};"); // 셀 크기 추가
                sb.Append($"{cell.orientation},{cell.blocked};"); // 셀 방향과 차단 여부 추가
            }

            using (SHA256 sha256 = SHA256.Create()) // SHA256 해시 알고리즘 생성
            {
                byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString()); // 문자열을 바이트 배열로 변환
                byte[] hash = sha256.ComputeHash(bytes); // 해시 값 계산
                
                StringBuilder result = new StringBuilder(); // 해시 값을 16진수 문자열로 변환
                for (int i = 0; i < 8; i++) // 앞의 8바이트만 사용
                {
                    result.Append(hash[i].ToString("x2")); // 16진수로 변환하여 추가
                }

                return $"sha256:{result}"; // 최종 해시 값 반환
            }
        }

        public string GetLastComputedHash() // 마지막으로 계산된 해시 값을 반환하는 메서드
        {
            return lastComputedHash; // 마지막 해시 값 반환
        }
    }
}
