using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Husky.Generator.WorkflowParser.YAML
{
    internal class YamlWorkflow: ParsedWorkflow
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string[] Dependencies { get; set; }

        [YamlMember(Alias = "Variables")]
        public Dictionary<string, string> GlobalVariables { get; set; }

        public Dictionary<string, YamlJob> Jobs { get; set; }


        public override ParsedWorkflow CreateWorkflow()
        {
            throw new NotImplementedException();
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