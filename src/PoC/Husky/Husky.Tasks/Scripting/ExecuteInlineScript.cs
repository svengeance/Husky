using System;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;

namespace Husky.Tasks.Scripting
{
    public class ExecuteInlineScript : HuskyTask<ExecuteInlineScriptOptions>
    {
        protected override void EnsureConfigured()
        {
            throw new NotImplementedException();
        }

        protected override void Execute()
        {
            throw new NotImplementedException();
        }

        protected override void Rollback()
        {
            throw new NotImplementedException();
        }

        public ExecuteInlineScript(ExecuteInlineScriptOptions configuration) : base(configuration)
        {
        }
    }
}