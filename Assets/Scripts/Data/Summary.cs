using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Summary
    {
        public int totalTargets; // 총 목표 수
        public int attempted; // 시도한 수
        public int success; // 성공한 수
        public int failed; // 실패한 수
        public Dictionary<ErrorCode, int> reasons; // 실패 이유별 카운트
        
        public Summary() // 생성자
        {
            totalTargets = 0;
            attempted = 0;
            success = 0;
            failed = 0;
            reasons = new Dictionary<ErrorCode, int>();
        }

        public void RecordSuccess() // 성공 기록
        {
            attempted++; // 시도한 수 증가
            success++; // 성공한 수 증가
        }

        // 실패 기록
        public void RecordFailure(ErrorCode errorCode)
        {
            attempted++; // 시도한 수 증가
            failed++; // 실패한 수 증가

            if (reasons.ContainsKey(errorCode)) // 이미 해당 오류 코드가 있으면 카운트 증가
            {
                reasons[errorCode]++; // 해당 오류 코드 카운트 증가
            }
            else
            {
                reasons[errorCode] = 1; // 해당 오류 코드 처음 등장, 카운트 1로 설정
            }
        }

        public override string ToString() // 요약 정보 문자열로 변환
        {
            var reasonStr = "";
            foreach (var reason in reasons) // 실패 이유들을 문자열로 변환
            {
                if (reasonStr.Length > 0)
                {
                    reasonStr += ", ";
                }
                reasonStr += $"{reason.Key}:{reason.Value}"; // 예: ROUTE_BLOCKED:3
            }
            
            return $"summary:\n" +
                   $"- total_targets: {totalTargets}\n" +
                   $"- attempted: {attempted}\n" +
                   $"- success: {success}\n" +
                   $"- failed: {failed}\n" +
                   $"- reasons: {{{reasonStr}}}";
        }
    }
}
