using System.Threading.Tasks;
using Husky.Core;
using NUnit.Framework;

namespace Husky.Installer.Tests.UninstallerTests
{
    internal class HuskyUninstallerTests: BaseInstallerTest
    {
        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Executed_workflow_has_preinstallation_stage_and_job()
        {
            // Arrange
            // Act
            await Uninstaller.Execute();

            // Assert
            Assert.AreEqual(HuskyConstants.WorkflowDefaults.PreUninstallation.StageName, Workflow.Stages[^1].Name);
            Assert.AreEqual(HuskyConstants.WorkflowDefaults.PreUninstallation.JobName, Workflow.Stages[^1].Jobs[0].Name);
        }

        protected override void ConfigureTestTaskOptions(TestHuskyTaskOptions options)
        {
            options.Title = "Hello!";
        }
    }
}