using Husky.Core.Workflow;

namespace Husky.Core.Exceptions
{
    internal class TaskConfigurationException
    {
        private readonly HuskyTask _task;

        public TaskConfigurationException(HuskyTask task)
        {
            _task = task;
            
        }
    }
}