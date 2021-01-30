using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Installation
{
    public class VerifyMachineMeetsRequirementsOptions: HuskyTaskConfiguration
    {
        public bool WarnInsteadOfHalt = false;

        internal override ValidationResult Validate() => new VerifyMachineMeetsRequirementsOptionsValidator().Validate(this);

        private class VerifyMachineMeetsRequirementsOptionsValidator : AbstractValidator<VerifyMachineMeetsRequirementsOptions> { }
    }
}