using Husky.Core.Enums;

namespace Husky.Core.Workflow
{
    public class HuskyStepConfiguration
    {
        public SupportedPlatforms SupportedPlatforms { get; }

        public HuskyStepConfiguration(SupportedPlatforms supportedPlatforms)
        {
            SupportedPlatforms = supportedPlatforms;
        }
    }
}