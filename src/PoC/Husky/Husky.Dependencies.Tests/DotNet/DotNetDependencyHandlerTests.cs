using System;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Dependencies.DependencyHandlers;
using Husky.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

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

        private DotNetDependencyHandler _sut = null!;
        private Mock<IShellExecutionService> _shellExecutionServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            _shellExecutionServiceMock = new Mock<IShellExecutionService>();

            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand("dotnet --list-runtimes"))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Join(Environment.NewLine, _runtimeOutput), string.Empty));

            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand("dotnet --list-sdks"))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Join(Environment.NewLine, _sdkOutput), string.Empty));

            _sut = new DotNetDependencyHandler(NullLogger<DotNetDependencyHandler>.Instance, _shellExecutionServiceMock.Object);
        }
        
        [TestCase(FrameworkInstallationType.Runtime, "dotnet --list-runtimes")]
        [TestCase(FrameworkInstallationType.Sdk, "dotnet --list-sdks")]
        [Category("UnitTest")]
        public void Dotnet_is_installed_check_runs_appropriate_shell_command(FrameworkInstallationType frameworkInstallationType, string expectedCommand)
        {
            // Arrange
            _shellExecutionServiceMock.Setup(s => s.ExecuteShellCommand(It.IsAny<string>()))
                                      .ReturnsAsync(new ShellExecutionService.ScriptExecutionResult(0, string.Empty, string.Empty));

            var dependency = new Core.Dependencies.DotNet("1.0.0", frameworkInstallationType, frameworkInstallationType == FrameworkInstallationType.Runtime
                ? Core.Dependencies.DotNet.RuntimeKind.AspNet
                : Core.Dependencies.DotNet.RuntimeKind.Sdk);
            
            // Act
            _ = _sut.IsAlreadyInstalled(dependency);
            
            // Assert
            _shellExecutionServiceMock.Verify(v => v.ExecuteShellCommand(expectedCommand), Times.Once);
        }

        [TestCase(">=5", true)]
        [TestCase("2.2.0", false)]
        [TestCase("<2.2.0", false)]
        [TestCase("3.1.x", true)]
        [Category("UnitTest")]
        public async ValueTask Dotnet_handler_identifies_installed_sdk(string range, bool expectedIsInstalledResult)
        {
            // Arrange
            var dependency = new Core.Dependencies.DotNet(range, FrameworkInstallationType.Sdk);

            // Act
            var isInstalled = await _sut.IsAlreadyInstalled(dependency);

            // Assert
            Assert.AreEqual(expectedIsInstalledResult, isInstalled);
        }

        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeKind.AspNet, true)]
        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeKind.RuntimeOnly, true)]
        [TestCase("2.x.x", Core.Dependencies.DotNet.RuntimeKind.Desktop, false)]
        [Category("UnitTest")]
        public async ValueTask Dotnet_handler_identifies_installed_runtime(string range, Core.Dependencies.DotNet.RuntimeKind runtimeKind, bool expectedIsInstalledResult)
        {
            // Arrange
            var dependency = new Core.Dependencies.DotNet(range, FrameworkInstallationType.Runtime, runtimeKind);

            // Act
            var isInstalled = await _sut.IsAlreadyInstalled(dependency);

            // Assert
            Assert.AreEqual(expectedIsInstalledResult, isInstalled);
        }
    }
}