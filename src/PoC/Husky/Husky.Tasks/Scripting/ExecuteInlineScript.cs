using System;
using Husky.Core.Workflow;

namespace Husky.Tasks.Scripting
{
    public class ExecuteInlineScript : HuskyTask
    {
        public string Script { get; set; }

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
    }
}