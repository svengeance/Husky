using System;
using System.Collections.Generic;
using System.Linq;

namespace Husky.Generator.WorkflowParser
{
    internal class ParsedWorkflow
    {
        public Dictionary<string, Dictionary<string, object?>> ConfigurationBlocks { get; set; } = new();
        public Dictionary<string, Dictionary<string, object?>> Dependencies { get; set; } = new();
        public Dictionary<string, object?> Variables { get; set; } = new();
        public Dictionary<string, ParsedStage> Stages { get; set; } = new();

        public void EnsureValidWorkflow()
        {
            foreach (var dependency in Dependencies)
                foreach (var dependencyProperty in dependency.Value.Where(dependencyProperty => dependencyProperty.Value is null))
                    throw new InvalidOperationException($"Dependencies must not be assigned a null value in any property: {dependency.Key}.{dependencyProperty.Key} was null");

            foreach (var variable in Variables.Where(w => w.Value is null))
                throw new InvalidOperationException($"Variables must not be assigned null value: {variable.Key} was null");

            foreach (var stage in Stages)
            {
                if (string.IsNullOrWhiteSpace(stage.Key))
                    throw new InvalidOperationException("Can not assign empty name to Stage");

                foreach (var job in stage.Value.Jobs)
                {
                    if (string.IsNullOrWhiteSpace(job.Key))
                        throw new InvalidOperationException("Can not assign empty name to Job");

                    foreach (var step in job.Value.Steps)
                        if (string.IsNullOrWhiteSpace(step.Key))
                            throw new InvalidOperationException("Can not assign empty name to Step");
                        else if (string.IsNullOrWhiteSpace(step.Value.Task))
                            throw new InvalidOperationException($"Steps must not be assigned a null Task: {stage.Key}.{job.Key}.{step.Key} was null.");
                }
            }

            if (Stages.Count(c => c.Key == GeneratorConstants.Workflow.DefaultStageName) > 1)
                throw new InvalidOperationException("Can not have more than one stage when Default Stage is present");

            if (Stages.Any(a => a.Value.Jobs.Count(c => c.Key == GeneratorConstants.Workflow.DefaultJobName) > 1))
                throw new InvalidOperationException("Can not have more than one job when Default Job is present");
        }
    }

    internal class ParsedStage
    {
        public Dictionary<string, ParsedJob> Jobs { get; set; } = new();
    }

    internal class ParsedJob
    {
        public string Os { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, ParsedStep> Steps { get; set; } = new();
    }

    internal class ParsedStep
    {
        public string Task { get; set; } = string.Empty;
        public string Os { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object?> With { get; set; } = new();
    }
}