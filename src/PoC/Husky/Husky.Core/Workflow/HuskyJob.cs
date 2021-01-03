using System.Collections.Generic;

namespace Husky.Core.Workflow
{
    public class HuskyJob
    {
        public string Name { get; }
        public List<HuskyStep<HuskyTaskConfiguration>> Steps { get; }

        public HuskyJob(string name)
        {
            Name = name;
            Steps = new List<HuskyStep<HuskyTaskConfiguration>>();
        }
    }
}