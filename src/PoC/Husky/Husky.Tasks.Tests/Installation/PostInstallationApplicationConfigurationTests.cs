using System;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskConfiguration.Installation;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Tasks.Installation;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using SemVer;
using static Husky.Core.HuskyConstants.RegistryKeys;

namespace Husky.Tasks.Tests.Installation
{
    public class PostInstallationApplicationConfigurationTests: BaseHuskyTaskUnitTest<PostInstallationApplicationRegistration>
    {
        [Test]
        [Category("UnitTest")]
        public async ValueTask Post_installation_calls_registry_write_with_subset_of_properties()
        {
            // Arrange
            var registryMock = _fixture.Create<Mock<IRegistryService>>();

            // Act
            await Sut.Execute();

            // Assert
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.DisplayName, "Jawbreakers"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.DisplayVersion, "25"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.Publisher, "Cul-de-sac"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.NoRemove, 1), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.Comments, "Every Ed's dream"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, AppUninstalls.RootKey, AppUninstalls.InstallDate, DateTime.Today.ToString("yyyyMMdd")), Times.Once());
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Post_installation_does_not_use_the_registry_when_not_on_windows()
        {
            // Arrange
            var registryMock = _fixture.Create<Mock<IRegistryService>>();
            PlatformInformationMock.Setup(s => s.OS).Returns(OS.Linux);

            // Act
            await Sut.Execute();

            // Assert
            registryMock.VerifyNoOtherCalls();
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration)
        {
            huskyConfiguration.Configure<ApplicationConfiguration>(app =>
            {
                app.Name = "Jawbreakers";
                app.Version = "25";
                app.Description = "Every Ed's dream";
            });
            
            huskyConfiguration.Configure<AuthorConfiguration>(author =>
            {
                author.Publisher = "Cul-de-sac";
            });
            
            huskyConfiguration.Configure<InstallationConfiguration>(install => install.AllowRemove = false);
        }

        protected override PostInstallationApplicationRegistrationConfiguration CreateDefaultTaskConfiguration() => new();
    }
}