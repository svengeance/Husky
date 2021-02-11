using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.Workflow;
using Husky.Tasks.Resources;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Resources
{
    public class ExtractBundledResourceTests: BaseFileSystemTest<ExtractBundledResource>
    {
        [Test]
        [Category("IntegrationTest")]
        public async Task Extract_bundled_resource_creates_identical_files_and_folders_from_embedded_resources()
        {
            // Arrange
            // Act
            await Sut.Execute();

            // Assert
            var availableResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var newFilePaths = TempDirectory.GetFiles("*", SearchOption.AllDirectories) // Standardize to forward slashes
                                             .Select(s => s.FullName.Replace(@"\", "/")) // for win/linux compatibility
                                             .ToList();

            Assert.AreEqual(availableResources.Length, newFilePaths.Count);

            foreach (var resource in availableResources)
            {
                var matchingNewFile = newFilePaths.Single(s => s.EndsWith(resource));
                var newFileRelativeDir = matchingNewFile.Substring(TempDirectory.FullName.Length + 1); // Remove the trailing slash (c:\temp -> c:\temp\)
                Assert.AreEqual(resource, newFileRelativeDir);
            }
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Extract_bundled_resource_cleans_existing_directories()
        {
            // Arrange
            UpdateTaskConfiguration<ExtractBundledResourceOptions>(conf => conf.CleanDirectories = true);
            var dirtyDirInfo = TempDirectory.CreateSubdirectory("NefariousDirectory/");

            // Act
            await Sut.Execute();

            // Assert
            DirectoryAssert.DoesNotExist(dirtyDirInfo);
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Extract_bundled_resource_cleans_existing_files()
        {
            // Arrange
            UpdateTaskConfiguration<ExtractBundledResourceOptions>(conf => conf.CleanFiles = true);
            var dirtyFileInfo = new FileInfo(Path.Combine(TempDirectory.FullName, "NefariousFile.txt"));
            await File.WriteAllTextAsync(dirtyFileInfo.FullName, "Nobody expects the Husky Inquisition!");

            // Act
            await Sut.Execute();

            // Assert
            FileAssert.DoesNotExist(dirtyFileInfo);
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Extract_bundled_resource_creates_single_file()
        {
            // Arrange
            UpdateTaskConfiguration<ExtractBundledResourceOptions>(conf => conf.Resources = "**/EmbeddedSample_1.txt");

            // Act
            await Sut.Execute();

            // Assert
            var expectedFileInfo = new FileInfo(Path.Combine(TempDirectory.FullName, "EmbeddedSample_1.txt"));
            FileAssert.Exists(expectedFileInfo);

            await using var expectedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EmbeddedSample_1.txt");
            await using var actualStream = expectedFileInfo.OpenRead();

            FileAssert.AreEqual(expectedStream, actualStream);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new ExtractBundledResourceOptions
        {
            TargetDirectory = TempDirectory.FullName,
            CleanDirectories = false,
            CleanFiles = false,
            Resources = "**/*"
        };
    }
}