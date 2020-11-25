using System;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask<CreateScriptFileOptions>
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

        public CreateScriptFile(CreateScriptFileOptions configuration) : base(configuration)
        {
        }
    }
}