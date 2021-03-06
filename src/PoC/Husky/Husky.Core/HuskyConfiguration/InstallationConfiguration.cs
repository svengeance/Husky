using FluentValidation;
using FluentValidation.Results;

namespace Husky.Core.HuskyConfiguration
{
    public record InstallationConfiguration: HuskyConfigurationBlock
    {
        public bool AddToRegistry { get; set; } = true;
        
        public bool AllowModify { get; set; } = false;
        public bool AllowRepair { get; set; } = false;
        public bool AllowRemove { get; set; } = true;

        public override ValidationResult Validate() => new InstallationConfigurationValidator().Validate(this);

        private class InstallationConfigurationValidator: AbstractValidator<InstallationConfiguration>
        {
            public InstallationConfigurationValidator() { }
        }
    }
}