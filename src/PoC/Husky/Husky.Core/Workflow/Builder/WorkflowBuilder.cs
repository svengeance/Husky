using System;
using Husky.Core.HuskyConfiguration;

namespace Husky.Core.Workflow.Builder
{
    public class WorkflowBuilder: IHuskyWorkflowBuilder
    {
        protected HuskyWorkflow Workflow { get; }

        internal WorkflowBuilder(HuskyWorkflow workflow) => Workflow = workflow;

        public IHuskyWorkflowBuilder Configure<T>(Action<T> configuration) where T : HuskyConfigurationBlock
        {
            Workflow.Configuration.Configure(configuration);

            return this;
        }

        public IHuskyWorkflowBuilder AddGlobalVariable(string key, string value)
        {
            Workflow.Variables[key] = value;

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
            => AddStage(HuskyConstants.Workflows.DefaultStageName, stageBuilderConfiguration);

        public IHuskyWorkflowBuilder WithDefaultStageAndJob(Action<IHuskyJobBuilder> jobBuilderConfiguration)
            => AddStage(HuskyConstants.Workflows.DefaultStageName, stage => stage.AddJob(HuskyConstants.Workflows.DefaultJobName, jobBuilderConfiguration));

        public IHuskyWorkflowBuilder AddDependency(HuskyDependency huskyDependency)
        {
            Workflow.Dependencies.Add(huskyDependency);
            return this;
        }

        public HuskyWorkflow Build() => Workflow;
    }

    public class StageBuilder : IHuskyStageBuilder
    {
        private readonly HuskyStage _stage;

        public StageBuilder(string stageName)
        {
            _stage = new HuskyStage(stageName);
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
        private HuskyStepConfiguration _defaultStepConfiguration;
        private readonly HuskyJob _job;

        public JobBuilder(string name, HuskyStepConfiguration? defaultStepConfiguration)
        {
            _defaultStepConfiguration = defaultStepConfiguration ?? HuskyStepConfiguration.DefaultConfiguration;
            _job = new HuskyJob(name);
        }

        public HuskyJob Build() => _job;

        public IHuskyJobBuilder SetDefaultStepConfiguration(HuskyStepConfiguration defaultStepConfiguration)
        {
            _defaultStepConfiguration = defaultStepConfiguration;

            return this;
        }

        public IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration) where TTaskConfiguration : HuskyTaskConfiguration
        {
            AddStep(name, taskConfiguration, _defaultStepConfiguration);

            return this;
        }

        public IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, HuskyStepConfiguration stepConfiguration) where TTaskConfiguration : HuskyTaskConfiguration
            => AddStep<TTaskConfiguration>(name, _ => { }, stepConfiguration);

        public IHuskyJobBuilder AddStep<TTaskConfiguration>(string name, Action<TTaskConfiguration> taskConfiguration, HuskyStepConfiguration stepConfiguration) where TTaskConfiguration : HuskyTaskConfiguration
        {
            var configuration = Activator.CreateInstance<TTaskConfiguration>();
            taskConfiguration.Invoke(configuration);

            var step = new HuskyStep<HuskyTaskConfiguration>(name, configuration, stepConfiguration);

            _job.Steps.Add(step);

            return this;
        }
    }
}