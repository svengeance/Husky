using System;
using Husky.Core.TaskConfiguration.Resources;
using Husky.Core.Workflow;

namespace Husky.Tasks.Resources
{
    public class ExtractBundledResource : HuskyTask<ExtractBundledResourceOptions>
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

        public ExtractBundledResource(ExtractBundledResourceOptions configuration) : base(configuration)
        {
        }
    }
}