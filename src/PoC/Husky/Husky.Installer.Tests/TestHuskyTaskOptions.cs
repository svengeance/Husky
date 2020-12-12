using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Installer.Tests
{
    public class TestHuskyTaskOptions : HuskyTaskConfiguration
    {
        public string? Title { get; set; }

        public bool HasValidated = false;

        internal override ValidationResult Validate()
        {
            HasValidated = true;
            return new TestHuskyTaskOptionsValidator().Validate(this);
        }

        private class TestHuskyTaskOptionsValidator : AbstractValidator<TestHuskyTaskOptions>
        {
            public TestHuskyTaskOptionsValidator()
            {
                RuleFor(r => r.Title).NotEmpty();
            }
        }
    }
}