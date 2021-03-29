using Husky.Generator.WorkflowParser;

namespace Husky.Generator.SourceWriter
{
    internal interface IHuskyWorkflowWriter<in T> where T: ParsedWorkflow
    {
        abstract string Write(T workflow);
    }
}