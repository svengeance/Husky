using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;
using Husky.Internal.Generator.Dictify;
using Husky.Services;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowValidator
    {
        Dictionary<string, object> ValidateWorkflow(HuskyWorkflow workflow);
    }

    public class WorkflowValidator: IWorkflowValidator
    {
        private readonly IVariableResolverService _variableResolverService;

        public WorkflowValidator(IVariableResolverService variableResolverService)
        {
            _variableResolverService = variableResolverService;
        }

        public Dictionary<string, object> ValidateWorkflow(HuskyWorkflow workflow)
        {
            var variables = workflow.ExtractAllVariables();
            _variableResolverService.ResolveVariables(variables);

            static void AppendExceptions(StringBuilder sb, IEnumerable<(string title, ValidationResult validation)> items)
                => items.Where(w => !w.validation.IsValid).Aggregate(sb, (prev, next) => prev.AppendLine(next.title).AppendLine(next.validation.ToString()));

            // Todo: Tasks which contain variables that are only computable at runtime may indeed be valid.
            var taskValidations = new List<(string, ValidationResult)>();
            foreach (var step in workflow.EnumerateSteps())
            {
                foreach (var (key, val) in ((IDictable)step.HuskyTaskConfiguration).ToDictionary())
                    variables[key] = val;

                var configuredStep = (HuskyTaskConfiguration)ObjectFactory.Create(step.HuskyTaskConfiguration.GetType(), variables);

                taskValidations.Add((
                    $"{step.Name}.{step.HuskyTaskConfiguration.GetType().Name} is not appropriately configured",
                    configuredStep.Validate()
                ));

                step.HuskyTaskConfiguration = configuredStep;
            }

            // Todo: Remove GetType().Name here
            var configurationValidations = workflow.Configuration.GetAllConfigurationTypes()
                                                   .Select(s => (HuskyConfigurationBlock)ObjectFactory.Create(s, variables))
                                                   .Select(s => ($"{s.GetType().Name} is not appropriately configured", s.Validate()));

            var exceptions = new StringBuilder();
            AppendExceptions(exceptions, taskValidations);
            AppendExceptions(exceptions, configurationValidations);

            if (exceptions.Length > 0)
                throw new ValidationException(exceptions.ToString());

            return variables;
        }
    }
}