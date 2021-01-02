using System.Collections.Generic;

namespace Husky.Generator.WorkflowParser
{
    internal abstract class ParsedWorkflow
    {
        public abstract ParsedWorkflow CreateWorkflow();
    }
}