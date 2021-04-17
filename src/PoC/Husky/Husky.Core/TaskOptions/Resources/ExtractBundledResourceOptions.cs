using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Resources
{
    public partial class ExtractBundledResourceOptions : HuskyTaskConfiguration
    {
        public string Resources { get; set; } = string.Empty;
        public string TargetDirectory { get; set; } = string.Empty;
        public bool CleanFiles { get; set; } = false;
        public bool CleanDirectories { get; set; } = false;

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