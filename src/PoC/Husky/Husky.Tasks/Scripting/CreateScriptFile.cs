using System;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
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