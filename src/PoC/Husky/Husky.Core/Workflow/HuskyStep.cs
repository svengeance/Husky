using System;
using System.Collections.Generic;
using Husky.Core.Enums;

namespace Husky.Core.Workflow
{
    public class HuskyStep<TTask> where TTask : HuskyTask
    {
        public string Name { get; set; }
        public SupportedPlatforms SupportedPlatforms { get; set; }
        
        internal TTask HuskyTask { get; set; }

        public void Configure(Action<TTask> taskConfiguration) => taskConfiguration.Invoke(HuskyTask);

        public HuskyStep(string name, TTask huskyTask)
        {
            Name = name;
            HuskyTask = huskyTask;
        }
    }
}