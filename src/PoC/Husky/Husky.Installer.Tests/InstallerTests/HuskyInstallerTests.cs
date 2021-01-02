using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Husky.Installer.Tests.InstallerTests
{
    public class HuskyInstallerTests: BaseInstallerTest
    {
        [Test]
        [Category("UnitTest")]
        public async Task Installer_validates_workflow()
        {
            // Arrange
            var installer = new HuskyInstaller(Workflow, cfg =>
            {
                cfg.ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() };
            });

            // Act
            await installer.Install();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions) Workflow.Stages[0].Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.True(testTaskOptions.HasValidated);
        }

        [Test]
        [Category("UnitTest")]

        public async Task Installer_replaces_variables_on_task_configuration()
        {
            // Arrange
            var expectedTitle = "Test - 4";

            // Act
            await Installer.Install();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions)Workflow.Stages[0].Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.AreEqual(expectedTitle, testTaskOptions.Title);
        }

        protected override void ConfigureTestTaskOptions(TestHuskyTaskOptions options)
        {
            options.Title = "Test - {random.RandomNumber}";
        }
    }
}