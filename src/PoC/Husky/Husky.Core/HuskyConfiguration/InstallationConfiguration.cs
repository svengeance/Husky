namespace Husky.Core.HuskyConfiguration
{
    public record InstallationConfiguration: HuskyConfigurationBlock
    {
        public bool AddToRegistry { get; set; } = true;
        
        public bool AllowModify { get; set; } = false;
        public bool AllowRepair { get; set; } = false;
        public bool AllowRemove { get; set; } = true;
    }
}