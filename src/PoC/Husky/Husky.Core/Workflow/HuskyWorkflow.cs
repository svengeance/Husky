using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
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

        public void Validate()
        {
            static void AppendExceptions(StringBuilder sb, IEnumerable<(string title, ValidationResult validation)> items)
                => items.Where(w => !w.validation.IsValid).Aggregate(sb, (prev, next) => prev.AppendLine(next.title).AppendLine(next.validation.ToString()));

            // Todo: Tasks which contain variables that are only computable at runtime may indeed be valid.
            var taskValidations = Stages.SelectMany(stage => stage.Jobs.SelectMany(job => job.Steps.Select(step =>
            (
                $"{step.Name}.{step.HuskyTaskConfiguration.GetType().Name} is not appropriately configured",
                step.HuskyTaskConfiguration.Validate()
            ))));

            // Todo: Configuration Blocks should have their variables resolved before they get here.
            var configurationValidations = Configuration.GetConfigurationBlocks()
                                                        .Select(s => ($"{s.GetType().Name} is not appropriately configured", s.Validate()));

            var exceptions = new StringBuilder();
            AppendExceptions(exceptions, taskValidations);
            AppendExceptions(exceptions, configurationValidations);

            if (exceptions.Length > 0)
                throw new ValidationException(exceptions.ToString());
        }

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
    }
}