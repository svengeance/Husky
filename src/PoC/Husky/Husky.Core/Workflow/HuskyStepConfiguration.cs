using Husky.Core.Enums;
using Husky.Core.Platform;

namespace Husky.Core.Workflow
{
    public class HuskyStepConfiguration
    {
        public OS Os { get; }

        // Without any specification, we can assume that the task is "any" operating system - it will work for our current platform.
        public static readonly HuskyStepConfiguration DefaultConfiguration = new(CurrentPlatform.OS);
        
        public HuskyStepConfiguration(OS os)
        {
            Os = os;
        }
    }
}