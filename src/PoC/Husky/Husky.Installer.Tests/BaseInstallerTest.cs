using Husky.Core.Builder;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Installer.Tests
{
    public abstract class BaseInstallerTest
    {
        protected HuskyWorkflow? Workflow { get; private set; }

        protected abstract TestHuskyTaskOptions TaskConfiguration { get; set; }

        [SetUp]
        public void BaseSetup()
        {
            Workflow = HuskyWorkflow.Create()
                                    .WithDefaultStageAndJob(job =>
                                         job.AddStep<TestHuskyTaskOptions>("TestStep", task => task.Title = "Testing Task")).Build();
        }
    }
}