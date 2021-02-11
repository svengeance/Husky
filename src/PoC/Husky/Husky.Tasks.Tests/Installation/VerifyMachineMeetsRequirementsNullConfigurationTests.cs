using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Tasks.Installation;
using Moq;
using NUnit.Framework;
using SemVer;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace Husky.Tasks.Tests.Installation
{
    public class VerifyMachineMeetsRequirementsNullConfigurationTests: BaseHuskyTaskUnitTest<VerifyMachineMeetsRequirements>
    {
        [SetUp]
        public void Setup()
        {
            PlatformInformationMock.Setup(s => s.LinuxDistribution).Returns(LinuxDistribution.Debian);
            PlatformInformationMock.Setup(s => s.OS).Returns(OS.Linux);
            PlatformInformationMock.Setup(s => s.OSVersion).Returns(new Version(5, 1, 0));
            PlatformInformationMock.Setup(s => s.OSArchitecture).Returns(Architecture.X64);

            _fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation
            {
                TotalMemoryMb = 100,
                DriveInformation = new[] { new SystemDriveInformation(new DirectoryInfo(@"C:\")) { FreeSpaceMb = 200 } }
            });
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_does_nothing_when_requires_configuration_is_null()
        {
            // Arrange
            _fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation
            {
                TotalMemoryMb = 1,
                DriveInformation = new[] { new SystemDriveInformation(new DirectoryInfo(@"C:\")) { FreeSpaceMb = 2 } }
            });

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.DoesNotThrowAsync(ExecuteTask);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration)
        {
            huskyConfiguration.Configure<ClientMachineRequirementsConfiguration>(config =>
            {
                config.FreeSpaceMb = null;
                config.MemoryMb = null;
                config.OsVersion = null;
                config.LinuxDistribution = LinuxDistribution.Unknown;
                config.SupportedOperatingSystems = Array.Empty<OS>();
            });
        }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration()
            => new VerifyMachineMeetsRequirementsOptions
            {
                WarnInsteadOfHalt = false
            };
    }
}