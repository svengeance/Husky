using System;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Utilities;
using Husky.Core.Workflow;

namespace Husky.Tasks.Utilities
{
    public class CreateShortcut : HuskyTask<CreateShortcutOptions>
    {
        protected override Task ExecuteTask()
        {
            throw new NotImplementedException();
        }

        protected override Task RollbackTask()
        {
            throw new NotImplementedException();
        }
    }
}