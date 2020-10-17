using System;

namespace Husky.Core.HuskyConfiguration
{
    public class ApplicationConfiguration: IHuskyConfigurationBlock
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "0.0.1";
        public HuskyDependency[] Dependencies { get; set; } = Array.Empty<HuskyDependency>();
    }
}