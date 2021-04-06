using System;
using System.Threading.Tasks;
using Husky.Dependencies.DependencyHandlers;
using Husky.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

using FrameworkInstallation = Husky.Core.Dependencies.DotNet.FrameworkInstallation;

namespace Husky.Dependencies.Tests.DotNet
{
    public class DotNetDependencyHandlerTests
    {
        private readonly string[] _runtimeOutput =
        {
            @"Microsoft.AspNetCore.All 2.1.23 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]",
            @"Microsoft.AspNetCore.App 2.1.23 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]",
            @"Microsoft.AspNetCore.App 3.1.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]",
            @"Microsoft.AspNetCore.App 5.0.0-preview.7.20365.19 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]",
            @"Microsoft.AspNetCore.App 5.0.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]",
            @"Microsoft.NETCore.App 2.1.23 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]",
            @"Microsoft.NETCore.App 3.1.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]",
            @"Microsoft.NETCore.App 5.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]",
            @"Microsoft.WindowsDesktop.App 3.1.9 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]",
            @"Microsoft.WindowsDesktop.App 5.0.0 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]"
        };

        private readonly string[] _sdkOutput =
        {
            @"5.0.100 [C:\Program Files\dotnet\sdk",
            @"3.1.405 [C:\Program Files\dotnet\sdk"
        };

        private Mock<IShellExecutionService> _shellExecutionServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            _shellExecutionServiceMock = new Mock<IShellExecutionService>();

            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand("dotnet --list-runtimes"))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Join(Environment.NewLine, _runtimeOutput), string.Empty));

            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand("dotnet --list-sdks"))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Join(Environment.NewLine, _sdkOutput), string.Empty));

        }
        
        [TestCase(FrameworkInstallation.Runtime, "dotnet --list-runtimes")]
        [TestCase(FrameworkInstallation.Sdk, "dotnet --list-sdks")]
        [Category("UnitTest")]
        public void Dotnet_is_installed_check_runs_appropriate_shell_command(FrameworkInstallation frameworkInstallationKind, string expectedCommand)
        {
            // Arrange
            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand(It.IsAny<string>()))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Empty, string.Empty));

            var dependency = new Core.Dependencies.DotNet("1.0.0", frameworkInstallationKind, frameworkInstallationKind == FrameworkInstallation.Runtime
                ? Core.Dependencies.DotNet.RuntimeInstallation.AspNet
                : Core.Dependencies.DotNet.RuntimeInstallation.Sdk);
            
            var sut = new DotNetDependencyHandler(dependency, NullLogger<DotNetDependencyHandler>.Instance, _shellExecutionServiceMock.Object);
            
            // Act
            _ = sut.IsAlreadyInstalled();
            
            // Assert
            _shellExecutionServiceMock.Verify(v => v.ExecuteShellCommand(expectedCommand), Times.Once);
        }

        [TestCase(">=5", true)]
        [TestCase("2.2.0", false)]
        [TestCase("<2.2.0", false)]
        [TestCase("3.1.x", true)]
        [Category("UnitTest")]
        public async Task Dotnet_handler_identifies_installed_sdk(string range, bool expectedIsInstalledResult)
        {
            // Arrange
            var dependency = new Core.Dependencies.DotNet(range, FrameworkInstallation.Sdk);
            var sut = new DotNetDependencyHandler(dependency, NullLogger<DotNetDependencyHandler>.Instance, _shellExecutionServiceMock.Object);

            // Act
            var isInstalled = await sut.IsAlreadyInstalled();

            // Assert
            Assert.AreEqual(expectedIsInstalledResult, isInstalled);
        }

        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeInstallation.AspNet, true)]
        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeInstallation.RuntimeOnly, true)]
        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeInstallation.Desktop, false)]
        [Category("UnitTest")]
        public async Task Dotnet_handler_identifies_installed_runtime(string range, Core.Dependencies.DotNet.RuntimeInstallation runtimeInstallation, bool expectedIsInstalledResult)
        {
            // Arrange
            var dependency = new Core.Dependencies.DotNet(range, FrameworkInstallation.Runtime, runtimeInstallation);
            var sut = new DotNetDependencyHandler(dependency, NullLogger<DotNetDependencyHandler>.Instance, _shellExecutionServiceMock.Object);

            // Act
            var isInstalled = await sut.IsAlreadyInstalled();

            // Assert
            Assert.AreEqual(expectedIsInstalledResult, isInstalled);
        }

        [Test]
        [Category("UnitTest")]
        public void Dotnet_handler_sastisfies_dotnet_aspnet_runtime_v5_dependency()
        {
            // Arrange
            var dependency = new Core.Dependencies.DotNet(">=5", FrameworkInstallation.Runtime, Core.Dependencies.DotNet.RuntimeInstallation.AspNet);
            var sut = new DotNetDependencyHandler(dependency, NullLogger<DotNetDependencyHandler>.Instance, _shellExecutionServiceMock.Object);

            // Act
            var satisfiesDependency = sut.TrySatisfyDependency(out var acquisitionMethod);

            // Assert
            Assert.True(satisfiesDependency);
            Assert.NotNull(acquisitionMethod);
        }
    }
}