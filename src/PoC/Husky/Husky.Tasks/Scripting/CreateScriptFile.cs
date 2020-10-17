using System;
using Husky.Core.Workflow;

namespace Husky.Tasks.Scripting
{
    public class CreateScriptFile : HuskyTask
    {
        public string Directory { get; set; }
        public string FileName { get; set; }
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