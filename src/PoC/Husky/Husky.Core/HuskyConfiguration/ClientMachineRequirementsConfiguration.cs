using System;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Enums;
using Range = SemanticVersioning.Range;

namespace Husky.Core.HuskyConfiguration
{
    public partial record ClientMachineRequirementsConfiguration : HuskyConfigurationBlock
    {
        public bool WarnInsteadOfHalt { get; set; } = false;
        public int? MemoryMb { get; set; }
        public long? FreeSpaceMb { get; set; }
        public Range? OsVersion { get; set; }
        public LinuxDistribution LinuxDistribution { get; set; } = LinuxDistribution.Unknown;
        public OS[] SupportedOperatingSystems { get; set; } = Array.Empty<OS>();

        public override ValidationResult Validate() => new ClientMachineRequirementsConfigurationValidator().Validate(this);

        private class ClientMachineRequirementsConfigurationValidator: AbstractValidator<ClientMachineRequirementsConfiguration>
        {
            public ClientMachineRequirementsConfigurationValidator()
            {
                RuleFor(r => r.MemoryMb).GreaterThan(0).When(w => w.MemoryMb.HasValue);
                RuleFor(r => r.FreeSpaceMb).GreaterThan(0).When(w => w.FreeSpaceMb.HasValue);
                RuleFor(r => r.LinuxDistribution).IsInEnum();
                RuleForEach(r => r.SupportedOperatingSystems).IsInEnum();
            }
        }
    }
}