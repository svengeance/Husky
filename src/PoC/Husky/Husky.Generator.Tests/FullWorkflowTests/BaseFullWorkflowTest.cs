using System.IO;
using Husky.Generator.WorkflowParser;
using NUnit.Framework;

namespace Husky.Generator.Tests.FullWorkflowTests
{
    internal abstract class BaseFullWorkflowTest<T> where T : IWorkflowParser, new()
    {
        protected abstract string FileName { get; }

        protected ParsedWorkflow ParsedWorkflow { get; private set; }

        protected string WorkflowResult { get; private set; }

        [OneTimeSetUp]
        public void BaseOneTimeSetup()
        {
            var parser = new T();

            var workflowFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, FileName);
            var rawWorkflow = File.ReadAllText(workflowFilePath);

            ParsedWorkflow = parser.ParseWorkflow(rawWorkflow);
            WorkflowResult = ParsedWorkflowWriter.Write(ParsedWorkflow);
        }
    }
}