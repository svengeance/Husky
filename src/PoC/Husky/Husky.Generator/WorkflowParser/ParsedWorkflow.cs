using System.Collections.Generic;
using Husky.Core.Workflow;

namespace Husky.Generator.WorkflowParser
{
    internal abstract class ParsedWorkflow
    {
        public abstract HuskyConfiguration GetHuskyConfiguration();

        public abstract ParsedWorkflow CreateWorkflow();
    }
}