﻿using System.Threading.Tasks;
using AutoFixture;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.TaskConfiguration.Installation;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Tasks.Installation;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using static Husky.Core.HuskyConstants.RegistryKeys;

namespace Husky.Tasks.Tests.Installation
{
    public class PostInstallationApplicationConfigurationNoRegistryTests : BaseHuskyTaskUnitTest<PostInstallationApplicationRegistration>
    {
        [Test]
        [Category("UnitTest")]
        public async ValueTask Post_installation_does_not_write_to_registry_when_specified_not_to()
        {
            // Arrange
            var registryMock = _fixture.Create<Mock<IRegistryService>>();

            // Act
            await Sut.Execute();

            // Assert
            registryMock.VerifyNoOtherCalls();
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration)
        {
            huskyConfiguration.Configure<ApplicationConfiguration>(app => { app.Name = "Ed Boy"; });
            huskyConfiguration.Configure<InstallationConfiguration>(install => install.AddToRegistry = false);
        }

        protected override PostInstallationApplicationRegistrationConfiguration CreateDefaultTaskConfiguration() => new();
    }
}