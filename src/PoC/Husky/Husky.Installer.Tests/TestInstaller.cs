using Husky.Core.Workflow;
using Husky.Installer.Lifecycle;

namespace Husky.Installer.Tests
{
    public class TestInstaller: HuskyInstaller
    {
        public TestInstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings) : base(workflow, installationSettings) { }

        protected override bool ShouldExecuteStep<T>(HuskyStep<T> step) => step.Name == "TestStep";
    }
}