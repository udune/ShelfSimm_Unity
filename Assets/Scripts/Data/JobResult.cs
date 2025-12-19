using System;

namespace Data
{
    public class JobResult
    {
        public string JobId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float TravelTimeSec { get; set; }
        public float HandleTimeSec { get; set; }
        public float TotalTimeSec { get; set; }
        public int PathLengthCells { get; set; }
        public string Result { get; set; }
        public string FailReason { get; set; }
        public string RobotName { get; set; }

        public JobResult(string jobId, DateTime startTime, DateTime endTime,
            float travelTimeSec, float handleTimeSec, float totalTimeSec,
            int pathLengthCells, string result, string failReason, string robotName)
        {
            JobId = jobId;
            StartTime = startTime;
            EndTime = endTime;
            TravelTimeSec = travelTimeSec;
            HandleTimeSec = handleTimeSec;
            TotalTimeSec = totalTimeSec;
            PathLengthCells = pathLengthCells;
            Result = result;
            FailReason = failReason;
            RobotName = robotName;
        }
    }
}
