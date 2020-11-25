using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Resources
{
    public class ExtractBundledResourceOptions : HuskyTaskConfiguration
    {
        public string? Resources { get; set; }
        public string? TargetDirectory { get; set; }
        public bool Clean { get; set; } = false;

        internal override ValidationResult Validate() => new ExtractBundledResourceOptionsValidator().Validate(this);

        private class ExtractBundledResourceOptionsValidator : AbstractValidator<ExtractBundledResourceOptions>
        {
            public ExtractBundledResourceOptionsValidator()
            {
                RuleFor(r => r.Resources).NotEmpty();
                RuleFor(r => r.TargetDirectory).NotEmpty();
            }
        }
    }
}