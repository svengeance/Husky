using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Installation
{
    public class PostInstallationApplicationRegistrationConfiguration: HuskyTaskConfiguration
    {
        internal override ValidationResult Validate() => new PostInstallationApplicationRegistrationConfigurationValidator().Validate(this);

        private class PostInstallationApplicationRegistrationConfigurationValidator: AbstractValidator<PostInstallationApplicationRegistrationConfiguration> { }
    }
}