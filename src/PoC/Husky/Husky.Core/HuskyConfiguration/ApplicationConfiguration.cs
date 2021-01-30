using System;

namespace Husky.Core.HuskyConfiguration
{
    public record ApplicationConfiguration: HuskyConfigurationBlock
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "0.0.1";
    }
}