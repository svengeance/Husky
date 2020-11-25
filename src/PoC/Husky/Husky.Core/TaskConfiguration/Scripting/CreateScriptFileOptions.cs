using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Scripting
{
    public class CreateScriptFileOptions : HuskyTaskConfiguration
    {
        public string? Directory { get; set; }
        public string? FileName { get; set; }
        public string? Script { get; set; }

        internal override ValidationResult Validate() => new CreateScriptFileOptionsValidator().Validate(this);

        private class CreateScriptFileOptionsValidator : AbstractValidator<CreateScriptFileOptions>
        {
            public CreateScriptFileOptionsValidator()
            {
                RuleFor(r => r.Directory).NotEmpty();
                RuleFor(r => r.FileName).NotEmpty();
                RuleFor(r => r.Script).NotEmpty();
            }
        }
    }
}