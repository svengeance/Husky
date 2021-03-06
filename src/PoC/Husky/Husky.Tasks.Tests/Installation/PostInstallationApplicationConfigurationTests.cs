using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
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
        [Platform("Win")]
        [SupportedOSPlatform("windows")]
        public async ValueTask Post_installation_calls_registry_write_with_subset_of_properties()
        {
            // Arrange
            var registryMock = Fixture.Create<Mock<IRegistryService>>();
            var appUninstallRootKey = $@"{AppUninstalls.RootKey}\Jawbreakers_Husky";

            // Act
            await Sut.Execute();

            // Assert
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.DisplayName, "Jawbreakers"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.DisplayVersion, "25"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.Publisher, "Cul-de-sac"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.NoRemove, 1), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.Comments, "Every Ed's dream"), Times.Once());
            registryMock.Verify(s => s.WriteKey(RegistryHive.LocalMachine, appUninstallRootKey, AppUninstalls.InstallDate, DateTime.Today.ToString("yyyyMMdd")), Times.Once());
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Post_installation_does_not_use_the_registry_when_not_on_windows()
        {
            // Arrange
            var registryMock = Fixture.Create<Mock<IRegistryService>>();
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

        protected override PostInstallationApplicationRegistrationOptions CreateDefaultTaskConfiguration() => new();
    }
}