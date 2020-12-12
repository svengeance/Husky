using System;
using System.Threading.Tasks;
using FluentValidation;

namespace Husky.Core.Workflow
{
    public abstract class HuskyTask<T> where T : HuskyTaskConfiguration
    {
        protected T? Configuration { get; private set; }

        public void SetConfiguration(T configuration)
        {
            if (Configuration is not null)
                throw new ApplicationException("Configuration was set twice. Please only set a configuration for a service instance once.");

            Configuration = configuration;
        }

        public Task Execute()
        {
            if (Configuration is null)
                throw new ApplicationException($"Configuration was not set - aborting execution.");

            return ExecuteTask();
        }

        public Task Rollback()
        {
            if (Configuration is null)
                throw new ApplicationException($"Configuration was not set - aborting rollback.");

            return RollbackTask();
        }

        protected abstract Task ExecuteTask();

        protected abstract Task RollbackTask();
    }
}