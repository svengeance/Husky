using Microsoft.CodeAnalysis.Text;

namespace Husky.Generator.WorkflowParser
{
    internal interface IWorkflowParser<out T> where T: ParsedWorkflow
    {
        T ParseWorkflow(string yamlWorkflow);
    }
}