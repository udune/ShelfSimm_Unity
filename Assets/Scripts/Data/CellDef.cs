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
        private int x;
        private int y;

        [Header("크기 (mm 단위)")]
        public int width = 90;
        public int height = 200;

        [Header("접근 설정")]
        public string orientation = "N";
        
        public int X => x;
        public int Y => y;

        public CellDef(string code, int width = 90, int height = 200, string orientation = "N")
        {
            this.code = code;
            this.width = width;
            this.height = height;
            this.orientation = orientation;
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

        public static bool TryParseCellCode(string cellCode, out int columnIndex, out int rowIndex)
        {
            columnIndex = 0;
            rowIndex = 0;

            if (string.IsNullOrWhiteSpace(cellCode))
            {
                return false;
            }

            cellCode = cellCode.Trim().ToUpper();

            int letterEndIndex = 0;
            while (letterEndIndex < cellCode.Length && char.IsLetter(cellCode[letterEndIndex]))
            {
                letterEndIndex++;
            }

            if (letterEndIndex == 0 || letterEndIndex == cellCode.Length)
            {
                return false;
            }

            string letters = cellCode.Substring(0, letterEndIndex);
            string numbers = cellCode.Substring(letterEndIndex);

            columnIndex = 0;
            for (int i = 0; i < letters.Length; i++)
            {
                columnIndex = columnIndex * 26 + (letters[i] - 'A' + 1);
            }
            columnIndex -= 1;

            if (!int.TryParse(numbers, out int rowNumber))
            {
                return false;
            }
            rowIndex = rowNumber - 1;

            return true;
        }

        public void SetPositionFromCode()
        {
            if (!TryParseCellCode(code, out int columnIndex, out int rowIndex))
            {
                return;
            }

            x = columnIndex;
            y = rowIndex;
        }
    }
}