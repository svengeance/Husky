﻿using System;
using System.Linq;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;

namespace Husky.Core.Builder
{
    public class WorkflowBuilder: IHuskyWorkflowBuilder
    {
        protected HuskyWorkflow Workflow { get; }

        internal WorkflowBuilder(HuskyWorkflow workflow) => Workflow = workflow;

        public IHuskyWorkflowBuilder Configure<T>(Action<T> configuration) where T : class, IHuskyConfigurationBlock
        {
            Workflow.Configuration.Configure(configuration);

            return this;
        }

        public IHuskyWorkflowBuilder AddStage(string name, Action<IHuskyStageBuilder> stageBuilderConfiguration)
        {
            var stageBuilder = new StageBuilder(name);
            stageBuilderConfiguration.Invoke(stageBuilder);

            Workflow.Stages.Add(stageBuilder.Build());

            return this;
        }

        public IHuskyWorkflowBuilder WithDefaultStage(Action<IHuskyStageBuilder> stageBuilderConfiguration)
            => AddStage(HuskyConstants.DefaultStageName, stageBuilderConfiguration);

        public IHuskyWorkflowBuilder WithDefaultStageAndJob(Action<IHuskyJobBuilder> jobBuilderConfiguration)
            => AddStage(HuskyConstants.DefaultStageName, stage => stage.AddJob(HuskyConstants.DefaultJobName, jobBuilderConfiguration));

        // Todo: Validate Workflow is.....valid
        public HuskyWorkflow Build() => Workflow;
    }

    public class StageBuilder : IHuskyStageBuilder
    {
        private readonly HuskyStage _stage;

        public StageBuilder(string stageName)
        {
            _stage = new HuskyStage(stageName);
        }

        public IHuskyStageBuilder SetDefaultStepConfiguration(HuskyStepConfiguration defaultStepConfiguration)
        {
            _stage.DefaultStepConfiguration = defaultStepConfiguration;

            return this;
        }

        public IHuskyStageBuilder AddJob(string name, Action<IHuskyJobBuilder> jobBuilderConfiguration)
        {
            var jobBuilder = new JobBuilder(name, _stage.DefaultStepConfiguration);
            jobBuilderConfiguration.Invoke(jobBuilder);

            _stage.Jobs.Add(jobBuilder.Build());

            return this;
        }

        public HuskyStage Build() => _stage;
    }

    public class JobBuilder: IHuskyJobBuilder
    {
        private readonly HuskyStepConfiguration? _defaultStepConfiguration;
        private readonly HuskyJob _job;

        public JobBuilder(string name, HuskyStepConfiguration? defaultStepConfiguration)
        {
            _defaultStepConfiguration = defaultStepConfiguration;
            _job = new HuskyJob(name);
        }

        public HuskyJob Build() => _job;

        public IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration) where TTaskConfiguration : HuskyTaskConfiguration
        {
            var configuration = Activator.CreateInstance<TTaskConfiguration>();
            taskConfiguration.Invoke(configuration);

            var step = new HuskyStep<HuskyTaskConfiguration>(name, configuration);
            step.HuskyStepConfiguration = _defaultStepConfiguration;

            _job.Steps.Add(step);

            return this;
        }

        public IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration, HuskyStepConfiguration stepConfiguration) where TTaskConfiguration : HuskyTaskConfiguration
        {
            AddStep(name, taskConfiguration);

            _job.Steps.Last().HuskyStepConfiguration = stepConfiguration;

            return this;
        }
    }
}