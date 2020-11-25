using System;
using System.IO;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskConfiguration.Utilities
{
    public class CreateShortcutOptions : HuskyTaskConfiguration
    {
        public string? ShortcutLocation { get; set; }
        public string? ShortcutName { get; set; }
        public string? Target { get; set; }

        internal override ValidationResult Validate() => new CreateShortcutOptionsValidator().Validate(this);

        private class CreateShortcutOptionsValidator : AbstractValidator<CreateShortcutOptions>
        {
            public CreateShortcutOptionsValidator()
            {
                RuleFor(r => r.ShortcutLocation).NotEmpty();
                RuleFor(r => r.ShortcutName).NotEmpty();
                RuleFor(r => r.Target).NotEmpty();
            }
        }

    }
}