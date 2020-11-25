using System;
using Husky.Core.TaskConfiguration.Utilities;
using Husky.Core.Workflow;

namespace Husky.Tasks.Utilities
{
    public class CreateShortcut : HuskyTask<CreateShortcutOptions>
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

        public CreateShortcut(CreateShortcutOptions configuration) : base(configuration)
        {

        }
    }
}