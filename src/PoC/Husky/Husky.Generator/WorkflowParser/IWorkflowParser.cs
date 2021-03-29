namespace Husky.Generator.WorkflowParser
{
    internal interface IWorkflowParser
    {
        ParsedWorkflow ParseWorkflow(string workflowText);
    }
}