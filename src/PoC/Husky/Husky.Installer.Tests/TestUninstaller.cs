using Husky.Core.Workflow;
using Husky.Installer.WorkflowExecution;

namespace Husky.Installer.Tests
{
    public class TestUninstaller: HuskyUninstaller
    {
        public TestUninstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings): base(workflow, installationSettings) { }
    }
}