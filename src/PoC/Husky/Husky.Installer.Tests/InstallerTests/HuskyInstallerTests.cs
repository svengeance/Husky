using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core;
using Moq;
using NUnit.Framework;

namespace Husky.Installer.Tests.InstallerTests
{
    public class HuskyInstallerTests: BaseInstallerTest
    {
        [Test]
        [Category("UnitTest")]
        public void Created_workflow_has_preinstallation_stage_and_job()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(HuskyConstants.Workflows.PreInstallation.DefaultStageName, Workflow.Stages[0].Name);
            Assert.AreEqual(HuskyConstants.Workflows.PreInstallation.DefaultJobName, Workflow.Stages[0].Jobs[0].Name);
        }

        [Test]
        [Category("UnitTest")]
        public void Created_workflow_has_postinstallation_stage_and_job()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(HuskyConstants.Workflows.PostInstallation.DefaultStageName, Workflow.Stages[^1].Name);
            Assert.AreEqual(HuskyConstants.Workflows.PostInstallation.DefaultJobName, Workflow.Stages[^1].Jobs[0].Name);
        }

        [Ignore("No longer safe to run with the addition of setting windows registry keys as part of the install process")]
        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Installer_validates_workflow()
        {
            // Arrange
            var installer = new HuskyInstaller(Workflow, cfg =>
            {
                cfg.ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() };
            });

            // Act
            await installer.Install();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions) Workflow.Stages.First(f => f.Name != HuskyConstants.Workflows.PreInstallation.DefaultStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.True(testTaskOptions.HasValidated);
        }

        [Ignore("No longer safe to run with the addition of setting windows registry keys as part of the install process")]
        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Installer_replaces_variables_on_task_configuration()
        {
            // Arrange
            var expectedTitle = "Test - 4";

            // Act
            await Installer.Install();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions)Workflow.Stages.First(f => f.Name != HuskyConstants.Workflows.PreInstallation.DefaultStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.AreEqual(expectedTitle, testTaskOptions.Title);
        }

        protected override void ConfigureTestTaskOptions(TestHuskyTaskOptions options)
        {
            options.Title = "Test - {random.RandomNumber}";
        }
    }
}