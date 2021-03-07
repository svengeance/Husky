using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Uninstallation
{
    public class ExecuteUninstallationOperationsOptions: HuskyTaskConfiguration
    {
        internal override ValidationResult Validate() => new ExecuteUninstallationOperationsOptionsValidator().Validate(this);

        private class ExecuteUninstallationOperationsOptionsValidator: AbstractValidator<ExecuteUninstallationOperationsOptions> { }
    }
}