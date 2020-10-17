using System;
using Husky.Core.Workflow;

namespace Husky.Tasks.Utilities
{
    public class CreateShortcut : HuskyTask
    {
        public string Target { get; set; }

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