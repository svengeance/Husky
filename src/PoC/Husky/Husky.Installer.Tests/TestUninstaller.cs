using Husky.Core.Workflow;
using Husky.Installer.Lifecycle;

namespace Husky.Installer.Tests
{
    public class TestUninstaller: HuskyUninstaller
    {
        public TestUninstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings): base(workflow, installationSettings) { }

        protected override bool ShouldExecuteStep<T>(HuskyStep<T> step) => step.Name == "TestStep";
    }
}