using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Husky.Installer.Tests.InstallerTests
{
    public class HuskyInstallerTests: BaseInstallerTest
    {
        protected override TestHuskyTaskOptions TaskConfiguration { get; set; }

        [Test]
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
            var testTask = (TestHuskyTaskOptions) Workflow!.Stages[0].Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.True(testTask.HasValidated);
        }
    }
}