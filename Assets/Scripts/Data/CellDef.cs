using System;
using UnityEngine;

namespace Data
{
    // 격자 내의 단일 칸 정의
    [Serializable]
    public class CellDef
    {
        [Header("기본 정보")] 
        public string code; // 칸 코드 (고유 식별자)

        [Header("위치")] 
        public int x; // 칸의 X 좌표 (격자 단위)
        public int y; // 칸의 Y 좌표 (격자 단위)
        
        [Header("크기 (mm 단위)")]
        public int width = 90; // 칸의 너비 (mm 단위)
        public int height = 200; // 칸의 높이 (mm 단위)
        
        [Header("AABB 충돌 판정 (셀 단위)")]
        public int tile_w = 1; // 칸의 너비 (셀 단위)
        public int tile_h = 1; // 칸의 높이 (셀 단위)
        
        [Header("접근 설정")]
        public string orientation = "N"; // 칸의 방향 (N, E, S, W)
        public string[] approach_priority; // 접근 우선순위 (예: ["N", "E", "S", "W"])
        
        [Header("상태")]
        public bool blocked = false; // 칸이 차단되었는지 여부 (true면 접근 불가)

        // 생성자
        public CellDef(string code, int x, int y, int width = 90, int height = 200, string orientation = "N")
        {
            this.code = code; // 칸 코드
            this.x = x; // 칸 X 좌표
            this.y = y; // 칸 Y 좌표
            this.width = width; // 칸 너비
            this.height = height; // 칸 높이
            this.orientation = orientation; // 칸 방향
            tile_w = 1; // 기본 너비 (셀 단위)
            tile_h = 1; // 기본 높이 (셀 단위)
            blocked = false; // 기본 차단 상태
        }

        // 책 두께(mm 단위)를 받아 이 칸에 수용 가능한 책의 최대 개수를 계산
        public int CalculateCapacity(int bookThickness)
        {
            if (bookThickness <= 0) // 두께가 0 이하인 경우
            {
                return 0; // 수용 불가
            }

            return Mathf.FloorToInt((float)width / bookThickness); // 칸 너비를 책 두께로 나누어 최대 수용 개수 계산
        }

        // 칸의 중심 좌표를 반환 (mm 단위)
        public override string ToString()
        {
            return $"Cell {code} at ({x}, {y}) - {width}x{height}mm"; // 칸 정보 문자열 반환
        }
    }
}