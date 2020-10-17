namespace Husky.Core.HuskyConfiguration
{
    public class AuthorConfiguration: IHuskyConfigurationBlock
    {
        public string Publisher { get; set; } = "Unknown Publisher";
        public string PublisherUrl { get; set; } = string.Empty;
    }
}