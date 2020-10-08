using Husky.Core.Workflow;

namespace Husky.Generator.WorkflowParser
{
    internal abstract class ParsedWorkflow
    {
        public abstract HuskyWorkflow CreateWorkflow();
    }
}