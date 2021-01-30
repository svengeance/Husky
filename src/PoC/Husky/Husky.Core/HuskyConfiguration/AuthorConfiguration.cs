namespace Husky.Core.HuskyConfiguration
{
    public record AuthorConfiguration: HuskyConfigurationBlock
    {
        public string Publisher { get; set; } = "Unknown Publisher";
        public string PublisherUrl { get; set; } = string.Empty;
    }
}