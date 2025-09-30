using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace Core
{
    // 칸과 거리 정보를 함께 담는 구조체
    [Serializable]
    public struct CellDistanceInfo
    {
        public CellDef cell; // 칸 정보
        public int distance; // 맨해튼 거리
    }
    
    // 로봇 위치에서 가장 가까운 N개의 칸을 선택하는 기능
    public class NearestCellSelector : MonoBehaviour
    {
        [Header("설정")] 
        [Range(1, 10)] 
        public int topN = 3; // 선택할 칸의 개수

        // 맨해튼 거리 계산
        // 공식: |x1 - x2| + |y1 - y2|
        public int CalculateDistance(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y); // 맨해튼 거리 계산
        }

        // 로봇 위치와 칸 목록을 받아 가장 가까운 N개의 칸을 거리순으로 반환
        public List<CellDistanceInfo> FilterTopN(Vector2Int robotPos, List<CellDef> targetCells)
        {
            var cellsWithDistance = new List<CellDistanceInfo>(); // 거리 정보를 담을 리스트
            foreach (var cell in targetCells) // 각 칸에 대해
            {
                var cellPos = new Vector2Int(cell.x, cell.y); // 칸 위치
                var distance = CalculateDistance(robotPos, cellPos); // 로봇과 칸 사이의 거리 계산
                cellsWithDistance.Add(new CellDistanceInfo // 거리 정보 추가
                {
                    cell = cell, // 칸 정보
                    distance = distance // 계산된 거리
                });
            }

            var sorted = cellsWithDistance // 거리순, 코드순으로 정렬
                .OrderBy(c => c.distance) // 거리 오름차순
                .ThenBy(c => c.cell.code) // 코드 오름차순
                .ToList(); // 리스트로 변환

            var topCells = sorted.Take(topN).ToList(); // 상위 N개 선택
            
            Debug.Log($"[NearestCellSelector] 로봇 위치: ({robotPos.x}, {robotPos.y})");
            Debug.Log($"Top {topN} 가까운 칸:");
            for (int i = 0; i < topCells.Count; i++) // 결과 출력
            {
                var info = topCells[i]; // 각 칸 정보
                Debug.Log($"{i + 1}. {info.cell.code} at ({info.cell.x}, {info.cell.y}) - 거리: {info.distance}");
            }
            
            return topCells; // 상위 N개 칸 반환
        }
    }
}
