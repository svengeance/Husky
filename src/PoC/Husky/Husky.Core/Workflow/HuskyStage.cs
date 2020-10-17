using System.Collections.Generic;

namespace Husky.Core.Workflow
{
    public class HuskyStage
    {
        public string Name { get; set; }
        public List<HuskyJob> Jobs { get; set; }

        public HuskyStage(string name)
        {
            Name = name;
            Jobs = new List<HuskyJob>();
        }
    }
}