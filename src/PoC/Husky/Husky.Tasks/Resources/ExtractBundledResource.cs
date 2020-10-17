using System;
using Husky.Core.Workflow;

namespace Husky.Tasks.Resources
{
    public class ExtractBundledResource : HuskyTask
    {
        public string Resources { get; set; }
        public string TargetDirectory { get; set; }
        public bool Clean { get; set; }

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