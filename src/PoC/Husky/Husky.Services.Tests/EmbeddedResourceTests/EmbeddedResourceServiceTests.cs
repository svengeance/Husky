using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Husky.Services.Tests.EmbeddedResourceTests
{
    public class EmbeddedResourceServiceTests : BaseUnitTest<EmbeddedResourceService>
    {
        private readonly Assembly _assemblyWithResources = Assembly.GetExecutingAssembly();

        [Test]
        [Category("UnitTest")]
        public void List_assembly_with_resources_lists_all_available_resources()
        {
            // Arrange
            var allResources = _assemblyWithResources.GetManifestResourceNames();
            
            // Act
            var filteredResources = Sut.ListResources(_assemblyWithResources);

            // Assert
            Assert.AreEqual(allResources.Length, filteredResources.Length);
        }

        [Test]
        [Category("UnitTest")]

        public void List_filters_resources_by_resource_name()
        {
            // Arrange
            var filter = "EmbeddedSample_2.txt";

            // Act
            var filteredResources = Sut.ListResources(_assemblyWithResources, filter).ToArray();

            // Assert
            Assert.AreEqual(1, filteredResources.Length);
            Assert.AreEqual(filter, filteredResources[0]);
        }

        [Test]
        [Category("UnitTest")]

        public void List_filters_resources_by_resource_directory()
        {
            // Arrange
            var filter = "**/SubDir/*.*";
            var expectedResourcePath = "SubDir/EmbeddedSample_SubDir_1.txt";
            
            // Act
            var filteredResources = Sut.ListResources(_assemblyWithResources, filter).ToArray();

            // Assert
            Assert.AreEqual(1, filteredResources.Length);
            Assert.AreEqual(expectedResourcePath, filteredResources[0]);
        }
    }
}