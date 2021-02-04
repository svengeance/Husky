using System;
using System.Collections.Generic;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;

namespace Husky.Core.Builder
{
    public interface IHuskyWorkflowBuilder
    {
        IHuskyWorkflowBuilder Configure<T>(Action<T> configuration) where T : HuskyConfigurationBlock;

        IHuskyWorkflowBuilder AddGlobalVariable(string key, string value);

        IHuskyWorkflowBuilder AddStage(string name, Action<IHuskyStageBuilder> stageBuilderConfiguration);

        IHuskyWorkflowBuilder WithDefaultStage(Action<IHuskyStageBuilder> stageBuilderConfiguration);

        IHuskyWorkflowBuilder WithDefaultStageAndJob(Action<IHuskyJobBuilder> jobBuilderConfiguration);
        
        IHuskyWorkflowBuilder AddDependency(HuskyDependency huskyDependency);

        HuskyWorkflow Build();
    }

    public interface IHuskyStageBuilder
    {
        IHuskyStageBuilder AddJob(string name, Action<IHuskyJobBuilder> jobBuilder);

    }

    public interface IHuskyJobBuilder
    {
        /*
         * Intention here is that clients will never explicitly add steps with zero configuration, however steps managed by the system may
         * be automatically added as a result of specific client configuration
         */
        internal IHuskyJobBuilder AddStep<TTaskConfiguration>(string name) where TTaskConfiguration : HuskyTaskConfiguration => AddStep<TTaskConfiguration>(name, _ => { });

        IHuskyJobBuilder SetDefaultStepConfiguration(HuskyStepConfiguration defaultStepConfiguration);
        
        IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration) where TTaskConfiguration : HuskyTaskConfiguration;

        IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration, HuskyStepConfiguration stepConfiguration) where TTaskConfiguration : HuskyTaskConfiguration;
    }
}