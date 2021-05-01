using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Workflow.Uninstallation;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Husky.Core.Tests.UninstallationOperationsTests
{
    public class ReadonlyUninstallOperationListTests
    {
        private readonly string _kittenString = "So many kittens.";

        private readonly string _uninstallOperationsFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        private FileInfo UninstallOperationsFileInfo => new(_uninstallOperationsFilePath);

        private ReadonlyUninstallOperationsList _sut = null!;

        private IUninstallOperationsList _uninstallOperationsList = null!;

        [SetUp]
        public async ValueTask Setup()
        {
            _uninstallOperationsList = await UninstallOperationsList.CreateOrRead(_uninstallOperationsFilePath);
            _uninstallOperationsList.AddEntry(UninstallOperationsList.EntryKind.File, _kittenString);
            await _uninstallOperationsList.Flush();

            _sut = new ReadonlyUninstallOperationsList(_uninstallOperationsList);
        }

        [TearDown]
        public void TearDown()
        {
            var uninstallOperationsFile = UninstallOperationsFileInfo;
            if (uninstallOperationsFile.Exists)
                uninstallOperationsFile.Delete();
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Adding_entry_to_read_only_operations_list_does_not_modify_underlying_file()
        {
            // Arrange
            _sut.AddEntry(UninstallOperationsList.EntryKind.RegistryKey, "So many puppies");
            await _sut.Flush();

            // Act
            var newOperationsList = await UninstallOperationsList.CreateOrRead(_uninstallOperationsFilePath);
            var readRegistryKeyOperations = newOperationsList.ReadEntries(UninstallOperationsList.EntryKind.RegistryKey);
            var readFileOperation = newOperationsList.ReadEntries(UninstallOperationsList.EntryKind.File).First();

            // Assert
            CollectionAssert.IsEmpty(readRegistryKeyOperations);
            Assert.AreEqual(_kittenString, readFileOperation);
        }

        [Test]
        [Category("UnitTest")]
        public void Adding_entry_to_read_only_operations_list_does_not_modify_in_memory_list()
        {
            // Arrange
            _sut.AddEntry(UninstallOperationsList.EntryKind.RegistryKey, "So many puppies");

            // Act
            var readRegistryKeyOperations = _uninstallOperationsList.ReadEntries(UninstallOperationsList.EntryKind.RegistryKey);
            var readFileOperation = _uninstallOperationsList.ReadEntries(UninstallOperationsList.EntryKind.File).First();

            // Assert
            CollectionAssert.IsEmpty(readRegistryKeyOperations);
            Assert.AreEqual(_kittenString, readFileOperation);
        }

        [Test]
        [Category("UnitTest")]
        public void Reading_entries_from_read_only_operations_reads_from_underlying_operations_list()
        {
            // Arrange
            var entryKind = UninstallOperationsList.EntryKind.File;
            var expectedValue = _kittenString;

            // Act
            var readValue = _sut.ReadEntries(entryKind).First();

            // Assert
            Assert.AreEqual(expectedValue, readValue);
        }
    }
}