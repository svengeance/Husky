using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Scripting
{
    public class ExecuteInlineScriptOptions : HuskyTaskConfiguration
    {
        public string? Script { get; set; }

        internal override ValidationResult Validate() => new ExecuteInlineScriptOptionsValidator().Validate(this);

        private class ExecuteInlineScriptOptionsValidator : AbstractValidator<ExecuteInlineScriptOptions>
        {
            public ExecuteInlineScriptOptionsValidator()
            {
                RuleFor(r => r.Script).NotEmpty();
            }
        }
    }
}