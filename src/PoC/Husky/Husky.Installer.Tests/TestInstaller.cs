using Husky.Core;
using Husky.Core.Workflow;
using Husky.Installer.WorkflowExecution;

namespace Husky.Installer.Tests
{
    public class TestInstaller: HuskyInstaller
    {
        public TestInstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings) : base(workflow, installationSettings) { }
    }
}