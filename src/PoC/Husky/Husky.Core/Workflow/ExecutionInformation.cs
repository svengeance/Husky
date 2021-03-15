using System;
using Husky.Core.Enums;

namespace Husky.Core.Workflow
{
    public class ExecutionInformation
    {
        public ExecutionStatus ExecutionStatus { get; set; } = ExecutionStatus.NotStarted;

        public DateTime? StartTime { get; set; }

        public DateTime? StopTime { get; set; }

        public TimeSpan Duration => StartTime == null
            ? TimeSpan.Zero
            : StopTime == null
                ? DateTime.Now - StartTime.Value
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

        public override string ToString()
            => ExecutionStatus switch
               {
                   ExecutionStatus.NotStarted => "Awaiting execution",
                   ExecutionStatus.Started    => $"Currently executing, {Duration:g} elapsed",
                   ExecutionStatus.Completed  => $"Completed execution, {Duration:g} elapsed",
                   ExecutionStatus.Error      => $"Errored during execution, {Duration:g} elapsed",
                   _                          => "Unknown state"
               };
    }
}