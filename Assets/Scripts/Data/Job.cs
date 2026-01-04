using System;

namespace Data
{
    public enum JobAction {
        PUT,    // Backward compat - maps to IN
        PICK,   // Backward compat - maps to OUT
        IN,
        OUT,
        REJECT
    }

    public static class JobActionConverter
    {
        public static string ToApiString(JobAction action)
        {
            return action switch
            {
                JobAction.PUT => "IN",
                JobAction.PICK => "OUT",
                JobAction.IN => "IN",
                JobAction.OUT => "OUT",
                JobAction.REJECT => "REJECT",
                _ => throw new ArgumentException($"Unknown action: {action}")
            };
        }

        public static JobAction FromApiString(string action)
        {
            return action?.ToUpper() switch
            {
                "PUT" => JobAction.IN,
                "PICK" => JobAction.OUT,
                "IN" => JobAction.IN,
                "OUT" => JobAction.OUT,
                "REJECT" => JobAction.REJECT,
                _ => throw new ArgumentException($"Unknown action: {action}")
            };
        }
    }

    public class Job
    {
        public string JobId { get; set; }
        public string MaterialId { get; set; }
        public string Code { get; set; }
        public JobAction Action { get; }
        public string CellCode { get; }
        public string MaterialName { get; }
        public int Quantity { get; }

        public Job(JobAction action, string cellCode, string materialName, int quantity, string jobId = null)
        {
            Action = action;
            CellCode = cellCode;
            MaterialName = materialName;
            Quantity = quantity;
            JobId = jobId;
        }
    }
}
