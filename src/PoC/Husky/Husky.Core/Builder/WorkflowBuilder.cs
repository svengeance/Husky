using System;
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

        public IHuskyStageBuilder AddJob(string name, Action<IHuskyJobBuilder> jobBuilderConfiguration)
        {
            var jobBuilder = new JobBuilder(name);
            jobBuilderConfiguration.Invoke(jobBuilder);

            _stage.Jobs.Add(jobBuilder.Build());

            return this;
        }

        public HuskyStage Build() => _stage;
    }

    public class JobBuilder: IHuskyJobBuilder
    {
        private readonly HuskyJob _job;

        public JobBuilder(string name)
        {
            _job = new HuskyJob(name);
        }

        public HuskyJob Build() => _job;

        public IHuskyJobBuilder AddStep<TTask>(string name, Action<IHuskyStepBuilder<HuskyStep<TTask>, TTask>> stepConfiguration) where TTask : HuskyTask
        {
            var stepBuilder = new StepBuilder<HuskyStep<TTask>, TTask>(name);
            stepConfiguration.Invoke(stepBuilder);

            _job.Steps.Add(stepBuilder.Build());

            return this;
        }
    }

    public class StepBuilder<TStep, TTask> : IHuskyStepBuilder<TStep, TTask> where TStep : HuskyStep<TTask> where TTask : HuskyTask
    {
        private readonly HuskyStep<HuskyTask> _step;

        public StepBuilder(string name)
        {
            var task = (TTask) Activator.CreateInstance(typeof(TTask)) as HuskyTask;
            _step = new HuskyStep<HuskyTask>(name, task);
        }

        public IHuskyStepBuilder<TStep, TTask> Configure(Action<TTask> taskConfiguration)
        {
            taskConfiguration.Invoke((TTask) _step.HuskyTask);

            return this;
        }

        public IHuskyStepBuilder<TStep, TTask> SupportedOn(SupportedPlatforms supportedPlatforms)
        {
            _step.SupportedPlatforms = supportedPlatforms;

            return this;
        }

        internal HuskyStep<HuskyTask> Build() => _step;
    }
}