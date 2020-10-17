using System.Collections.Generic;

namespace Husky.Core.Workflow
{
    public class HuskyJob
    {
        public string Name { get; set; }
        public List<HuskyStep<HuskyTask>> Steps { get; set; }

        public HuskyJob(string name)
        {
            Name = name;
            Steps = new List<HuskyStep<HuskyTask>>();
        }
    }
}