using FluentValidation;
using FluentValidation.Results;

namespace Husky.Core.HuskyConfiguration
{
    public partial record ApplicationConfiguration: HuskyConfigurationBlock
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string SupportTelephone { get; set; } = string.Empty;
        public string SupportUrl { get; set; } = string.Empty;
        public string AboutUrl { get; set; } = string.Empty;

        public string Version { get; set; } = "0.0.1";
        
        public string IconFile { get; set; } = string.Empty;

        public string InstallDirectory { get; set; } = string.Empty;

        public override ValidationResult Validate() => new ApplicationConfiguration.ApplicationConfigurationValidator().Validate(this);

        private class ApplicationConfigurationValidator: AbstractValidator<ApplicationConfiguration>
        {
            public ApplicationConfigurationValidator()
            {
                RuleFor(r => r.Name).NotEmpty();
                RuleFor(r => r.Description).NotNull();
                RuleFor(r => r.Version).NotNull();
                RuleFor(r => r.IconFile).NotNull();
                RuleFor(r => r.InstallDirectory).NotEmpty();

                // Todo: GH #12 Need to have variable resolutions for these Configuration blocks
                //RuleFor(r => r.InstallDirectory).NotEmpty().Must(m => new DirectoryInfo(m).Parent != null).WithMessage("Can not install into root directory");
            }
        }
    }
}

