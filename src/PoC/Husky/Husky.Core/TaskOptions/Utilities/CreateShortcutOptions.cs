using System.IO;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Core.TaskOptions.Utilities
{
    public class CreateShortcutOptions : HuskyTaskConfiguration
    {
        public string ShortcutLocation { get; set; } = string.Empty;
        public string ShortcutName { get; set; } = string.Empty;
        public string? ShortcutImageFilePath { get; set; }
        public string Target { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public string UnixShortcutType { get; set; } = HuskyConstants.UnixDesktopFileTypes.Application;
        public string UnixShortcutCategories { get; set; } = HuskyConstants.UnixDesktopFileCategories.Main.Utility;
        public string UnixShortcutUrl { get; set; } = string.Empty;
        public bool UnixStartUseTerminal { get; set; } = false;

        internal override ValidationResult Validate() => new CreateShortcutOptionsValidator().Validate(this);

        private class CreateShortcutOptionsValidator : AbstractValidator<CreateShortcutOptions>
        {
            public CreateShortcutOptionsValidator()
            {
                RuleFor(r => r.ShortcutLocation).NotEmpty();
                RuleFor(r => r.ShortcutName).NotEmpty().Must(m => !Path.HasExtension(m));
                RuleFor(r => r.Target).NotEmpty();
                
                RuleFor(r => r.UnixShortcutType).Must(m => m == HuskyConstants.UnixDesktopFileTypes.Application ||
                                                            m == HuskyConstants.UnixDesktopFileTypes.Link ||
                                                            m == HuskyConstants.UnixDesktopFileTypes.Directory);

                RuleFor(r => r.UnixShortcutUrl).NotEmpty().When(w => w.UnixShortcutType == HuskyConstants.UnixDesktopFileTypes.Link);
            }
        }
    }
}