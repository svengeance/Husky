using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Workflow.Uninstallation;
using NUnit.Framework;

namespace Husky.Core.Tests.UninstallationOperationsTests
{
    public class UninstallOperationsTests
    {
        private readonly string _uninstallOperationsFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        private UninstallOperationsList _sut = null!;
        
        private FileInfo UninstallOperationsFileInfo => new(_uninstallOperationsFilePath);


        [SetUp]
        public async ValueTask Setup()
        {
            _sut = (UninstallOperationsList) await UninstallOperationsList.CreateOrRead(_uninstallOperationsFilePath);
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
        public void Uninstall_operations_can_create_a_new_file()
        {
            // Arrange
            // Act
            // Assert
            Assert.NotNull(_sut);
            FileAssert.Exists(UninstallOperationsFileInfo);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Uninstall_operations_can_parse_file_and_read_all_entry_kinds()
        {
            // Arrange
            HashSet<string> GenerateRandomEntries() => Enumerable.Range(0, 10).Select(_ => Guid.NewGuid().ToString()).ToHashSet();

            var writableContent = Enum.GetValues<UninstallOperationsList.EntryKind>()
                                       .Select(s => (kind: s, entries: GenerateRandomEntries()))
                                       .ToArray();

            // Act
            foreach (var content in writableContent)
            foreach (var entry in content.entries)
                _sut.AddEntry(content.kind, entry);

            await _sut.Flush();

            var parsedOperations = await UninstallOperationsList.CreateOrRead(_uninstallOperationsFilePath);

            // Assert
            foreach (var entryKind in Enum.GetValues<UninstallOperationsList.EntryKind>())
            {
                var entries = parsedOperations.ReadEntries(entryKind).ToHashSet();
                var expectedEntries = writableContent.First(f => f.kind == entryKind).entries;
                CollectionAssert.AreEqual(expectedEntries, entries);
            }
        }

        [Test]
        [Category("IntegrationTest")]
        public void Uninstall_operations_can_write_and_subsequently_read_all_entry_kinds_in_memory()
        {
            // Arrange 
            static HashSet<string> GenerateRandomEntries()
                => Enumerable.Range(0, 10).Select(_ => Guid.NewGuid().ToString()).ToHashSet();

            var writableContent = Enum.GetValues<UninstallOperationsList.EntryKind>()
                                      .Select(s => (kind: s, entries: GenerateRandomEntries()))
                                      .ToArray();

            // Act
            foreach (var content in writableContent)
            foreach (var entry in content.entries)
                _sut.AddEntry(content.kind, entry);

            // Assert
            foreach (var entryKind in Enum.GetValues<UninstallOperationsList.EntryKind>())
            {
                var entries = _sut.ReadEntries(entryKind).ToHashSet();
                var expectedEntries = writableContent.First(f => f.kind == entryKind).entries;
                CollectionAssert.AreEqual(expectedEntries, entries);
            }
        }
    }
}