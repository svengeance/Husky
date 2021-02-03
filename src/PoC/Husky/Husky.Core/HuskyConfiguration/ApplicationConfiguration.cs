using System;

namespace Husky.Core.HuskyConfiguration
{
    public record ApplicationConfiguration: HuskyConfigurationBlock
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string SupportTelephone { get; set; } = string.Empty;
        public string SupportUrl { get; set; } = string.Empty;
        public string AboutUrl { get; set; } = string.Empty;

        public string Version { get; set; } = "0.0.1";
        
        public string IconFile { get; set; } = string.Empty;

        public string InstallDirectory { get; set; } = string.Empty;
    }
}