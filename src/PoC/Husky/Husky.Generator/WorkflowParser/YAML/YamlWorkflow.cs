using System.Collections.Generic;
using System.Text;
using Husky.Core.Workflow;
using YamlDotNet.Serialization;

namespace Husky.Generator.WorkflowParser.YAML
{
    internal class YamlWorkflow: ParsedWorkflow
    {
        public string Name { get; set; }
        public decimal Version { get; set; }
        public string Publisher { get; set; }
        public string[] Dependencies { get; set; }

        [YamlMember(Alias = "Variables")]
        public Dictionary<string, string> GlobalVariables { get; set; }

        public Dictionary<string, YamlJob> Jobs { get; set; }

        public override HuskyWorkflow CreateWorkflow()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class YamlJob
    {
        public List<YamlStep> Steps { get; set; }
    }

    internal class YamlStep
    {
        public string Name { get; set; }
        public string[] Platforms { get; set; } 
        public string Task { get; set; }
        public Dictionary<string, string> With { get; set; }
        public Dictionary<string, string> Output{ get; set; }
    }
}