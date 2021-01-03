using System.Collections.Generic;

namespace Husky.Core.Workflow
{
    public class HuskyStage
    {
        public string Name { get; }

        public HuskyStepConfiguration? DefaultStepConfiguration { get; set; }

        public List<HuskyJob> Jobs { get; }

        public HuskyStage(string name)
        {
            Name = name;
            Jobs = new List<HuskyJob>();
        }
    }
}