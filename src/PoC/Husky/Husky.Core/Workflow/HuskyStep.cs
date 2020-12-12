using System;
using Husky.Core.Enums;

namespace Husky.Core.Workflow
{
    public class HuskyStep<TTaskConfiguration> where TTaskConfiguration : HuskyTaskConfiguration
    {
        public string Name { get; set; }
        
        internal HuskyStepConfiguration? HuskyStepConfiguration { get; set; }
        internal TTaskConfiguration HuskyTaskConfiguration { get; set; }
        internal ExecutionInformation ExecutionInformation { get; set; } = new();

        public HuskyStep(string name, TTaskConfiguration huskyTaskConfiguration)
        {
            Name = name;
            HuskyTaskConfiguration = huskyTaskConfiguration;
        }
    }
}