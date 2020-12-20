using System;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow;

namespace Husky.Tasks
{
    public abstract class HuskyTask<T> where T : HuskyTaskConfiguration
    {
        protected T? Configuration { get; private set; }
        protected InstallationContext? InstallationContext { get; private set; }
        protected ExecutionInformation? ExecutionInformation { get; private set; }

        public void SetExecutionContext(T configuration, InstallationContext installationContext, ExecutionInformation executionInformation)
        {
            if (Configuration is not null || InstallationContext is not null || ExecutionInformation is not null)
                throw new ApplicationException("Execution context was set twice, when it should have only been set once.");

            Configuration = configuration;
            InstallationContext = installationContext;
            ExecutionInformation = executionInformation;
        }

        public Task Execute()
        {
            if (Configuration is null || InstallationContext is null || ExecutionInformation is null)
                throw new ApplicationException($"Task was not configured - aborting execution.");

            return ExecuteTask();
        }

        public Task Rollback()
        {
            if (Configuration is null || InstallationContext is null || ExecutionInformation is null)
                throw new ApplicationException($"Task was not configured - aborting rollback.");

            return RollbackTask();
        }

        protected abstract Task ExecuteTask();

        protected abstract Task RollbackTask();
    }
}