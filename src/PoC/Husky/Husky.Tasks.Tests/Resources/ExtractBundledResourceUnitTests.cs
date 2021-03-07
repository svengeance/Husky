using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Husky.Tasks.Resources;
using Moq;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Resources
{
    public class ExtractBundledResourceUnitTests: BaseHuskyTaskUnitTest<ExtractBundledResource>
    {
        [Test]
        [Category("UnitTest")]
        public async Task Extract_bundled_resource_creates_identical_files_and_folders_from_embedded_resources()
        {
            // Arrange
            var mockResources = new[] { "Puppies.txt", "Kitty/Kittens.txt" };
            var mockPuppyContent = "Puppies, puppies everywhere!";
            var mockPuppyResourceStream = new MemoryStream(Encoding.UTF8.GetBytes(mockPuppyContent));
            var mockKittenContent = "Kittens, kittens everywhere!";
            var mockKittenResourceStream = new MemoryStream(Encoding.UTF8.GetBytes(mockKittenContent));

            var embeddedResourceMock = Fixture.Create<Mock<IEmbeddedResourceService>>();
            embeddedResourceMock.Setup(s => s.ListResources(It.IsAny<Assembly>(), It.IsAny<string>())).Returns(mockResources);
            embeddedResourceMock.Setup(s => s.RetrieveResource(It.IsAny<Assembly>(), mockResources[0])).Returns(mockPuppyResourceStream);
            embeddedResourceMock.Setup(s => s.RetrieveResource(It.IsAny<Assembly>(), mockResources[1])).Returns(mockKittenResourceStream);

            // Act
            await Sut.Execute();

            // Assert
            var expectedFilePaths = mockResources.Select(s => Path.Combine("AdoptionCenter", s)).ToArray();
            UninstallOperationsMock.Verify(v => v.AddEntry(UninstallOperationsList.EntryKind.File, It.Is<string>(i => expectedFilePaths.Contains(i))), Times.Exactly(2));

            var expectedDirectories = new[] { "AdoptionCenter", Path.Combine("AdoptionCenter", "Kitty") };
            UninstallOperationsMock.Verify(v => v.AddEntry(UninstallOperationsList.EntryKind.Directory, It.Is<string>(i => expectedDirectories.Contains(i))), Times.AtLeast(2));
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new ExtractBundledResourceOptions
        {
            TargetDirectory = "AdoptionCenter",
            CleanDirectories = false,
            CleanFiles = false,
            Resources = "**/*"
        };
    }
}