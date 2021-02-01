using System;
using Husky.Core.Enums;
using Range = SemVer.Range;

namespace Husky.Core.HuskyConfiguration
{
    public record ClientMachineRequirementsConfiguration: HuskyConfigurationBlock
    {
        public bool WarnInsteadOfHalt { get; set; } = false;
        public int? MemoryMb { get; set; }
        public long? FreeSpaceMb { get; set; }
        public Range? OsVersion { get; set; }
        public LinuxDistribution LinuxDistribution { get; set; } = LinuxDistribution.Unknown;
        public OS[] SupportedOperatingSystems { get; set; } = Array.Empty<OS>();
    }
}