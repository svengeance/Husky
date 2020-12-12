using System;
using Husky.Core.Enums;

namespace Husky.Core
{
    public class ExecutionInformation
    {
        public ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.NotStarted;

        public DateTime? StartTime { get; set; }

        public DateTime? StopTime { get; set; }

        public TimeSpan Duration => StartTime == null
            ? TimeSpan.Zero
            : StopTime == null
                ? TimeSpan.MaxValue
                : StartTime.Value - StopTime.Value;

        public void Start()
        {
            StartTime = DateTime.Now;
            ExecutionStatus = ExecutionStatus.Completed;
        }

        public void Finish()
        {
            StopTime = DateTime.Now;
            ExecutionStatus = ExecutionStatus.Completed;
        }

        public void Fail()
        {
            StopTime = DateTime.Now;
            ExecutionStatus = ExecutionStatus.Error;
        }
    }
}