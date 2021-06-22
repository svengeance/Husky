using System;
using System.Threading.Tasks;
using Husky.Core;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Husky.Installer.Tests.InstallerSettingsTests
{
    public class InstallerSettingsArgParsingTests
    {
        private HuskyInstallerSettings Sut { get; set; } = null!;

        [SetUp]
        public void Setup()
        {
            Sut = new HuskyInstallerSettings();
        }

        [TestCase(HuskyConstants.StepTags.Install, HuskyConstants.StepTags.Install)]
        [TestCase(HuskyConstants.StepTags.Uninstall, HuskyConstants.StepTags.Uninstall)]
        [TestCase(HuskyConstants.StepTags.Modify, HuskyConstants.StepTags.Modify)]
        [TestCase(HuskyConstants.StepTags.Repair, HuskyConstants.StepTags.Repair)]
        [TestCase("", HuskyConstants.StepTags.Install)]
        [Category("UnitTest")]
        public void Parsing_args_determines_steps_to_use(string verb, string expectedTag)
        {
            // Arrange
            // Act
            var parseResult = Sut.LoadFromStartArgs(new[] { verb });

            // Assert
            Assert.Zero(parseResult);
            Assert.AreEqual(Sut.TagToExecute, expectedTag);
        }

        [Test]
        [Category("UnitTest")]
        public void Parsing_unknown_verb_returns_non_zero_code()
        {
            // Arrange
            var args = "Unknown Command";

            // Act
            var parseResult = Sut.LoadFromStartArgs(new[] { args });

            // Assert
            Assert.NotZero(parseResult);
        }

        [TestCase("--dry-run", nameof(HuskyInstallerSettings.IsDryRun), true)]
        [TestCase("--verbosity=Trace", nameof(HuskyInstallerSettings.LogLevel), LogLevel.Trace)]
        [TestCase("--verbosity=Error", nameof(HuskyInstallerSettings.LogLevel), LogLevel.Error)]
        [Category("UnitTest")]
        public void Parsing_global_options_from_args_configures_install_settings(string arg, string expectedProperty, object expectedValue)
        {
            // Arrange
            object? GetPropertyValue(HuskyInstallerSettings obj, string name)
                => obj.GetType().GetProperty(expectedProperty)?.GetValue(obj);

            // Act
            var parseResult = Sut.LoadFromStartArgs(new[] { "install", arg });

            // Assert
            Assert.Zero(parseResult);

            var propValue = GetPropertyValue(Sut, expectedProperty);
            Assert.AreEqual(expectedValue, propValue);
        }
    }
}