using System;
using Husky.Core.Enums;
using Husky.Core.Platform;

namespace Husky.Core.Workflow
{
    public class HuskyStepConfiguration
    {
        public OS Os { get; }

        public string[] Tags { get; init; } = Array.Empty<string>();
        
        // Without any specification, we can assume that the task is "any" operating system - it will work for our current platform.
        public static readonly HuskyStepConfiguration DefaultConfiguration = new(CurrentPlatform.OS)
        {
            Tags = HuskyConstants.StepTags.DefaultStepTags
        };

        public HuskyStepConfiguration(OS os)
        {
            Os = os;
        }

        public HuskyStepConfiguration(OS os, params string[] tags)
        {
            Os = os;
            Tags = tags;
        }
    }
}