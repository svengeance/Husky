using System.Collections.Generic;

namespace Husky.Generator.WorkflowParser
{
    internal class ParsedWorkflow
    {
        public Dictionary<string, Dictionary<string, object?>> ConfigurationBlocks { get; set; } = new();
        public Dictionary<string, Dictionary<string, object?>> Dependencies { get; set; } = new();
        public Dictionary<string, object?> Variables { get; set; } = new();
        public Dictionary<string, ParsedStage> Stages { get; set; } = new();
    }

    internal class ParsedStage
    {
        public Dictionary<string, ParsedJob> Jobs { get; set; } = new();
    }

    internal class ParsedJob
    {
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, ParsedStep> Steps { get; set; } = new();
    }

    internal class ParsedStep
    {
        public List<string> Platforms { get; set; } = new();
        public string Task { get; set; } = string.Empty;
        public Dictionary<string, object?> With { get; set; } = new();
    }
}