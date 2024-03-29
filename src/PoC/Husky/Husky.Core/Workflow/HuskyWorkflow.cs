﻿using System;
using System.Collections.Generic;
using System.Linq;
using Husky.Core.Dependencies;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow.Builder;

namespace Husky.Core.Workflow
{
    public sealed class HuskyWorkflow
    {
        public HuskyConfiguration Configuration { get; }
        public Dictionary<string, object> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<HuskyStage> Stages { get; } = new();
        public List<HuskyDependency> Dependencies { get; } = new();

        private HuskyWorkflow(HuskyConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IHuskyWorkflowBuilder Create() => new WorkflowBuilder(new HuskyWorkflow(HuskyConfiguration.Create()));

        public void Reverse()
        {
            Stages.Reverse();
            foreach (var stage in Stages)
            {
                stage.Jobs.Reverse();
                foreach (var job in stage.Jobs)
                    job.Steps.Reverse();
            }
        }

        public Dictionary<string, object> ExtractAllVariables()
            => new(HuskyVariables.AsDictionary()
                                 .Concat(Configuration.ExtractConfigurationBlockVariables())
                                 .Concat(Variables));

        public IEnumerable<HuskyStep<HuskyTaskConfiguration>> EnumerateSteps()
            => Stages.SelectMany(stage => stage.Jobs.SelectMany(job => job.Steps));
    }
}