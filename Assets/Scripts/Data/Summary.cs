using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Summary
    {
        public int totalTargets;
        public int attempted;
        public int success;
        public int failed;
        public Dictionary<ErrorCode, int> reasons;

        public Summary()
        {
            totalTargets = 0;
            attempted = 0;
            success = 0;
            failed = 0;
            reasons = new Dictionary<ErrorCode, int>();
        }

        public void RecordSuccess()
        {
            attempted++;
            success++;
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            attempted++;
            failed++;

            if (reasons.ContainsKey(errorCode))
            {
                reasons[errorCode]++;
            }
            else
            {
                reasons[errorCode] = 1;
            }
        }

        public override string ToString()
        {
            var reasonStr = "";
            foreach (var reason in reasons)
            {
                if (reasonStr.Length > 0)
                {
                    reasonStr += ", ";
                }
                reasonStr += $"{reason.Key}:{reason.Value}";
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
