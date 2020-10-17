using System;
using System.Collections.Generic;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;

namespace Husky.Core.Builder
{
    public interface IHuskyWorkflowBuilder
    {
        IHuskyWorkflowBuilder Configure<T>(Action<T> configuration) where T : class, IHuskyConfigurationBlock;

        IHuskyWorkflowBuilder AddStage(string name, Action<IHuskyStageBuilder> stageBuilderConfiguration);

        IHuskyWorkflowBuilder WithDefaultStage(Action<IHuskyStageBuilder> stageBuilderConfiguration);

        IHuskyWorkflowBuilder WithDefaultStageAndJob(Action<IHuskyJobBuilder> jobBuilderConfiguration);

        HuskyWorkflow Build();
    }

    public interface IHuskyStageBuilder
    {
        IHuskyStageBuilder AddJob(string name, Action<IHuskyJobBuilder> jobBuilder);
    }

    public interface IHuskyJobBuilder
    {
        IHuskyJobBuilder AddStep<TTask>(string name, Action<IHuskyStepBuilder<HuskyStep<TTask>, TTask>> stepConfiguration) where TTask : HuskyTask;
    }

    public interface IHuskyStepBuilder<TStep, out TTask> where TStep: HuskyStep<TTask> where TTask: HuskyTask
    {
        IHuskyStepBuilder<TStep, TTask> SupportedOn(SupportedPlatforms supportedPlatforms);

        IHuskyStepBuilder<TStep, TTask> Configure(Action<TTask> taskConfiguration);
    }
}