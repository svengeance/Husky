using System.Collections.Generic;

namespace Husky.Core.Workflow
{
    public class HuskyWorkflow
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string[] Dependencies { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public Dictionary<string, HuskyJob> Jobs { get; set; }
    }
}