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
        [Category("IntegrationTest")]
        public void Created_workflow_has_preinstallation_stage_and_job()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(HuskyConstants.PreInstallation.DefaultPreInstallationStageName, Workflow.Stages[0].Name);
            Assert.AreEqual(HuskyConstants.PreInstallation.DefaultPreInstallationJobName, Workflow.Stages[0].Jobs[0].Name);
        }

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
            var testTaskOptions = (TestHuskyTaskOptions) Workflow.Stages.First(f => f.Name != HuskyConstants.PreInstallation.DefaultPreInstallationStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.True(testTaskOptions.HasValidated);
        }

        [Test]
        [Category("IntegrationTest")]

        public async ValueTask Installer_replaces_variables_on_task_configuration()
        {
            // Arrange
            var expectedTitle = "Test - 4";

            // Act
            await Installer.Install();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions)Workflow.Stages.First(f => f.Name != HuskyConstants.PreInstallation.DefaultPreInstallationStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.AreEqual(expectedTitle, testTaskOptions.Title);
        }

        protected override void ConfigureTestTaskOptions(TestHuskyTaskOptions options)
        {
            options.Title = "Test - {random.RandomNumber}";
        }
    }
}