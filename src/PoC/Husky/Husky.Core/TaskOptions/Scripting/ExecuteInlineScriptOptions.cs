using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Scripting
{
    public class ExecuteInlineScriptOptions : HuskyTaskConfiguration
    {
        public string Script { get; set; } = string.Empty;
        public bool IsWindowVisible { get; set; } = false;

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