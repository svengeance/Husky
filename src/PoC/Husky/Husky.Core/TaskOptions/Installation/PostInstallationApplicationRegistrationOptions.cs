using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Installation
{
    public class PostInstallationApplicationRegistrationOptions: HuskyTaskConfiguration
    {
        internal override ValidationResult Validate() => new PostInstallationApplicationRegistrationConfigurationValidator().Validate(this);

        private class PostInstallationApplicationRegistrationConfigurationValidator: AbstractValidator<PostInstallationApplicationRegistrationOptions> { }
    }
}