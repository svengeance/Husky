﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow.Builder;

namespace Husky.Core.Workflow
{
    public sealed class HuskyWorkflow
    {
        public HuskyConfiguration Configuration { get; }
        public Dictionary<string, string> Variables { get; } = new();
        public List<HuskyStage> Stages { get; } = new();
        public List<HuskyDependency> Dependencies { get; } = new();

        private HuskyWorkflow(HuskyConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IHuskyWorkflowBuilder Create() => new WorkflowBuilder(new HuskyWorkflow(HuskyConfiguration.Create()));

        public void Validate()
        {
            var validations = Stages.SelectMany(stage => stage.Jobs.SelectMany(job => job.Steps.Select(step => new
            {
                Validation = step.HuskyTaskConfiguration.Validate(),
                TaskName = step.HuskyTaskConfiguration.GetType().Name,
                StepName = step.Name,
                JobName = job.Name,
                StageName = stage.Name          
            })));

            var exceptionString = validations.Where(w => !w.Validation.IsValid)
                                             .Aggregate(new StringBuilder(), (sb, next) => sb.AppendLine($"{next.StageName}.{next.JobName}.{next.StepName}.{next.TaskName}")
                                                                                             .Append(next.Validation))
                                             .ToString();

            if (!string.IsNullOrEmpty(exceptionString))
                throw new ValidationException(exceptionString);
        }
    }
}