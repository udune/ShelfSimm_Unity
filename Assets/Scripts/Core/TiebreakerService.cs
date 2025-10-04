using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    // 타이브레이커 서비스
    public class TiebreakerService
    {
        private readonly TiebreakerConfig config; // 타이브레이커 설정
        private System.Random random; // 랜덤 생성기
        
        public TiebreakerService(TiebreakerConfig config) // 생성자
        {
            this.config = config; // 설정 저장
            InitializeRandom(config.randomSeed); // 랜덤 초기화
        }
        
        public void InitializeRandom(int seed) // 랜덤 초기화
        {
            random = new System.Random(seed); // 시드값으로 랜덤 생성기 초기화

            if (config.enableLogging) // 로깅이 활성화된 경우
            {
                Debug.Log($"[TiebreakerService] Random initialized with seed {seed}");
            }
        }

        // 그룹 내에서 타이브레이커 적용
        public List<CellDistanceInfo> ApplyTiebreaker(List<CellDistanceInfo> candidates)
        {
            if (candidates == null || candidates.Count == 0) // 후보가 없으면 그대로 반환
            {
                return candidates; // 빈 리스트 반환
            }
            
            var groups = candidates // 거리별로 그룹화
                .GroupBy(c => c.actualPathCost) // 실제 경로 비용으로 그룹화
                .OrderBy(g => g.Key) // 비용이 낮은 순서로 정렬
                .ToList(); // 그룹 리스트로 변환
            
            var result = new List<CellDistanceInfo>(); // 최종 결과 리스트

            foreach (var group in groups) // 각 그룹에 대해
            {
                var groupList = group.ToList(); // 그룹을 리스트로 변환
                
                if (groupList.Count == 1) // 그룹에 하나만 있으면 그대로 추가
                {
                    result.Add(groupList[0]); // 하나만 추가
                }
                else
                {
                    var sorted = ApplyTiebreakerToGroup(groupList); // 타이브레이커 적용
                    result.AddRange(sorted); // 정렬된 그룹 추가
                }
            }

            return result; // 최종 결과 반환
        }

        private List<CellDistanceInfo> ApplyTiebreakerToGroup(List<CellDistanceInfo> group) // 그룹 내에서 타이브레이커 적용
        {
            switch (config.mode) // 설정된 모드에 따라 처리
            {
                case TiebreakerConfig.TiebreakerMode.Alphabetical: // 알파벳순
                    return ApplyAlphabeticalTiebreaker(group); // 알파벳순 적용
                case TiebreakerConfig.TiebreakerMode.Random: // 랜덤
                    return ApplyRandomTiebreaker(group); // 랜덤 적용
                default:
                    return group; // 기본적으로 그대로 반환
            }
        }

        private List<CellDistanceInfo> ApplyAlphabeticalTiebreaker(List<CellDistanceInfo> group) // 알파벳순 타이브레이커
        {
            var sorted = group.OrderBy(c => c.cell.code).ToList(); // 코드 오름차순 정렬

            if (config.enableLogging && group.Count > 1) // 로깅이 활성화된 경우
            {
                var codes = string.Join(", ", sorted.Select(c => c.cell.code)); // 정렬된 코드들
                Debug.Log($"[TiebreakerService] Alphabetical tiebreaker applied: {codes}");
            }

            return sorted; // 정렬된 리스트 반환
        }

        private List<CellDistanceInfo> ApplyRandomTiebreaker(List<CellDistanceInfo> group) // 랜덤 타이브레이커
        {
            var shuffled = new List<CellDistanceInfo>(group); // 그룹 복사
            int n = shuffled.Count; // 그룹 크기
            
            for (int i = n - 1; i > 0; i--) // 피셔-예이츠 셔플 알고리즘
            {
                int j = random.Next(i + 1); // 0부터 i까지 랜덤 인덱스 선택
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]); // 두 요소 교환
            }

            if (config.enableLogging && group.Count > 1) // 로깅이 활성화된 경우
            {
                var codes = string.Join(", ", shuffled.Select(c => c.cell.code)); // 섞인 코드들
                Debug.Log($"[TiebreakerService] Random tiebreaker applied: {codes}");
            }

            return shuffled; // 섞인 리스트 반환
        }
    }
}