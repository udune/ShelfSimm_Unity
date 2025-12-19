using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class Summary
    {
        public int total;
        public int attempt;
        public int success;
        public int fail;
        public Dictionary<ErrorCode, int> reasons;

        public Summary()
        {
            total = 0;
            attempt = 0;
            success = 0;
            fail = 0;
            reasons = new Dictionary<ErrorCode, int>();
        }

        public void RecordSuccess()
        {
            attempt++;
            success++;
        }

        public void RecordFailure(ErrorCode errorCode)
        {
            attempt++;
            fail++;

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
                   $"- total_targets: {total}\n" +
                   $"- attempted: {attempt}\n" +
                   $"- success: {success}\n" +
                   $"- failed: {fail}\n" +
                   $"- reasons: {{{reasonStr}}}";
        }
    }
}
