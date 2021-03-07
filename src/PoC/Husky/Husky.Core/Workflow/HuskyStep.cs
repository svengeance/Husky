namespace Husky.Core.Workflow
{
    public class HuskyStep<TTaskConfiguration> where TTaskConfiguration : HuskyTaskConfiguration
    {
        public string Name { get; }

        internal HuskyStepConfiguration HuskyStepConfiguration { get; }
        internal TTaskConfiguration HuskyTaskConfiguration { get; }
        internal ExecutionInformation ExecutionInformation { get; } = new();

        public HuskyStep(string name, TTaskConfiguration huskyTaskConfiguration, HuskyStepConfiguration huskyStepConfiguration)
        {
            Name = name;
            HuskyTaskConfiguration = huskyTaskConfiguration;
            HuskyStepConfiguration = huskyStepConfiguration;
        }
    }
}