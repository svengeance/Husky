using System;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow;

namespace Husky.Tasks
{
    public abstract class HuskyTask<T> where T : HuskyTaskConfiguration
    {
        /*
         * Todo: Find a way to make the properties in each HuskyTask's Configuration readonly.
         *       While we obviously want users to be able to set the properties (and the Action<T> syntax isn't compatible with init;),
         *       we do not want to commit some sin like changing a Task's configuration at runtime.
         *
         *       That would be horrible.
         */
        protected T Configuration { get; private set; } = null!;
        protected HuskyContext HuskyContext { get; private set; } = null!;
        protected ExecutionInformation ExecutionInformation { get; private set; } = null!;

        public void SetExecutionContext(T configuration, HuskyContext huskyContext, ExecutionInformation executionInformation)
        {
            if (Configuration is not null || HuskyContext is not null || ExecutionInformation is not null)
                throw new ApplicationException("Execution context was set twice, when it should have only been set once.");

            Configuration = configuration;
            HuskyContext = huskyContext;
            ExecutionInformation = executionInformation;
        }

        public ValueTask Execute()
        {
            if (Configuration is null || HuskyContext is null || ExecutionInformation is null)
                throw new ApplicationException($"Task was not configured - aborting execution.");

            return ExecuteTask();
        }

        protected abstract ValueTask ExecuteTask();
    }
}