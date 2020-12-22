using Husky.Core.Enums;

namespace Husky.Core.Workflow
{
    public class HuskyStepConfiguration
    {
        public SupportedPlatforms SupportedPlatforms { get; }

        public static HuskyStepConfiguration DefaultConfiguration = new(SupportedPlatforms.All);
        
        public HuskyStepConfiguration(SupportedPlatforms supportedPlatforms)
        {
            SupportedPlatforms = supportedPlatforms;
        }
    }
}