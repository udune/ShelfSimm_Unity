using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CellDef
    {
        [Header("기본 정보")]
        public string code;

        [Header("위치")]
        public int x;
        public int y;

        [Header("크기 (mm 단위)")]
        public int width = 90;
        public int height = 200;

        [Header("AABB 충돌 판정 (셀 단위)")]
        public int tile_w = 1;
        public int tile_h = 1;

        [Header("접근 설정")]
        public string orientation = "N";
        public string[] approach_priority;

        [Header("상태")]
        public bool blocked = false;

        public CellDef(string code, int x, int y, int width = 90, int height = 200, string orientation = "N")
        {
            this.code = code;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.orientation = orientation;
            tile_w = 1;
            tile_h = 1;
            blocked = false;
        }

        public int CalculateCapacity(int bookThickness)
        {
            if (bookThickness <= 0)
            {
                return 0;
            }
            return Mathf.FloorToInt((float)width / bookThickness);
        }

        public override string ToString()
        {
            return $"Cell {code} at ({x}, {y}) - {width}x{height}mm";
        }
    }
}