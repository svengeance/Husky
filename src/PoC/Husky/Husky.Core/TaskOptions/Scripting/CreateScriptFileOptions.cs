using System.IO;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Scripting
{
    public partial class CreateScriptFileOptions : HuskyTaskConfiguration
    {
        public string Directory { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;

        internal override ValidationResult Validate() => new CreateScriptFileOptionsValidator().Validate(this);

        private class CreateScriptFileOptionsValidator : AbstractValidator<CreateScriptFileOptions>
        {
            public CreateScriptFileOptionsValidator()
            {
                RuleFor(r => r.Directory).NotEmpty();
                RuleFor(r => r.FileName).NotEmpty().Must(m => !Path.HasExtension(m));
                RuleFor(r => r.Script).NotEmpty();
            }
        }
    }
}