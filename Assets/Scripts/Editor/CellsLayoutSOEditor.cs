using System;
using System.Collections.Generic;
using System.Text;
using Data.Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CellsLayoutSO))] // CellsLayoutSO에 대한 커스텀 에디터
    public class CellsLayoutSOEditor : UnityEditor.Editor // Unity 에디터용 클래스
    {
        private SerializedProperty layoutHashProp; // layout_hash 프로퍼티

        private void OnEnable() // 에디터가 활성화될 때 호출
        {
            layoutHashProp = serializedObject.FindProperty("layout_hash"); // layout_hash 프로퍼티 찾기
        }

        public override void OnInspectorGUI() // 인스펙터 GUI 그리기
        {
            serializedObject.Update(); // 직렬화된 오브젝트 업데이트
            
            DrawDefaultInspector(); // 기본 인스펙터 그리기

            EditorGUILayout.Space(); // 공간 추가
            EditorGUILayout.LabelField("Layout Hash", EditorStyles.boldLabel); // 레이아웃 해시 라벨
            
            EditorGUI.BeginDisabledGroup(true); // 비활성화된 상태로 시작
            EditorGUILayout.TextField("Current Hash", layoutHashProp.stringValue); // 현재 해시 값 표시
            EditorGUI.EndDisabledGroup(); // 비활성화된 상태로 끝
            
            if (GUILayout.Button("Recalculate Hash")) // 해시 재계산 버튼
            {
                RecalculateHash(); // 해시 재계산
            }
            
            serializedObject.ApplyModifiedProperties(); // 변경된 프로퍼티 적용
        }

        private void RecalculateHash() // 해시 재계산 메서드
        {
            CellsLayoutSO layout = target as CellsLayoutSO; // 대상 오브젝트를 CellsLayoutSO로 캐스팅
            if (layout == null) // null 체크
            {
                return; // null이면 종료
            }
            
            string newHash = ComputeLayoutHash(layout); // 새로운 해시 값 계산

            layout.layout_hash = newHash; // 레이아웃의 해시 값 업데이트
            EditorUtility.SetDirty(layout); // 레이아웃 오브젝트를 더티 상태로 표시 (변경 사항 저장)
            
            Debug.Log($"[CellsLayoutSOEditor] Layout hash recalculated: {newHash}");
        }
        
        private string ComputeLayoutHash(CellsLayoutSO layout) // 레이아웃 정보를 기반으로 해시 값을 계산하는 메서드
        {
            StringBuilder sb = new StringBuilder(); // 문자열 빌더 생성
            sb.Append($"{layout.schema_version}"); // 스키마 버전 추가
            sb.Append($"{layout.grid_size.x},{layout.grid_size.y};"); // 그리드 크기 추가
            sb.Append($"{layout.warehouse.x},{layout.warehouse.y};"); // 창고 위치 추가

            var sortedCells = new List<CellDef>(layout.cells); // 셀 목록 복사
            sortedCells.Sort((a, b) => string.Compare(a.code, b.code, StringComparison.Ordinal)); // 셀 코드를 기준으로 정렬

            foreach (var cell in sortedCells) // 각 셀에 대해
            {
                sb.Append($"{cell.code},{cell.x},{cell.y};"); // 셀 코드와 위치 추가
                sb.Append($"{cell.width},{cell.height};"); // 셀 크기 추가
                sb.Append($"{cell.orientation},{cell.blocked}"); // 셀 방향과 차단 여부 추가
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create()) // SHA256 해시 알고리즘 생성
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
    }
}