using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Tasks.Installation;
using Moq;
using NUnit.Framework;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace Husky.Tasks.Tests.Installation
{
    public class VerifyMachineMeetsRequirementsTests: BaseHuskyTaskUnitTest<VerifyMachineMeetsRequirements>
    {
        [SetUp]
        public void Setup()
        {
            PlatformInformationMock.Setup(s => s.LinuxDistribution).Returns(LinuxDistribution.Debian);
            PlatformInformationMock.Setup(s => s.OS).Returns(OS.Linux);
            PlatformInformationMock.Setup(s => s.OSVersion).Returns(new Version(5, 1, 0));
            PlatformInformationMock.Setup(s => s.OSArchitecture).Returns(Architecture.X64);

            Fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation
            {
                TotalMemoryMb = 100,
                DriveInformation = new[] { new SystemDriveInformation(new DirectoryInfo(@"C:\")) { FreeSpaceMb = 200 } }
            });
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_ensures_client_os_is_supported()
        {
            // Arrange
            Fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation());
            PlatformInformationMock.Setup(s => s.OS).Returns(OS.Osx);

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.ThrowsAsync<NotSupportedException>(ExecuteTask);
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_ensures_client_os_version_is_supported()
        {
            // Arrange
            PlatformInformationMock.Setup(s => s.OSVersion).Returns(new Version(4, 9, 0));

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.ThrowsAsync<ApplicationException>(ExecuteTask);
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_ensures_client_linux_distribution_is_supported()
        {
            // Arrange
            PlatformInformationMock.Setup(s => s.LinuxDistribution).Returns(LinuxDistribution.CentOS);

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.ThrowsAsync<ApplicationException>(ExecuteTask);
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_ensures_client_has_enough_ram()
        {
            // Arrange
            Fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation
            {
                TotalMemoryMb = 2,
                DriveInformation = new[] { new SystemDriveInformation(new DirectoryInfo(@"C:\")) { FreeSpaceMb = 200 } }
            });

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.ThrowsAsync<ApplicationException>(ExecuteTask);
        }

        [Test]
        [Category("UnitTest")]
        public void Verification_ensures_client_has_enough_hard_drive_space()
        {
            // Arrange
            Fixture.Create<Mock<ISystemService>>().Setup(s => s.GetSystemInformation()).ReturnsAsync(new SystemInformation
            {
                TotalMemoryMb = 200,
                DriveInformation = new[] { new SystemDriveInformation(new DirectoryInfo(@"C:\")) { FreeSpaceMb = 2 } }
            });

            // Act
            async Task ExecuteTask() => await Sut.Execute();

            // Assert
            Assert.ThrowsAsync<ApplicationException>(ExecuteTask);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration)
        {
            huskyConfiguration.Configure<ClientMachineRequirementsConfiguration>(config =>
            {
                config.FreeSpaceMb = 10;
                config.MemoryMb = 20;
                config.OsVersion = Range.Parse(">=5");
                config.LinuxDistribution = LinuxDistribution.Debian;
                config.SupportedOperatingSystems = new[] { OS.Linux };
            });
        }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration()
            => new VerifyMachineMeetsRequirementsOptions
            {
                WarnInsteadOfHalt = false
            };
    }
}