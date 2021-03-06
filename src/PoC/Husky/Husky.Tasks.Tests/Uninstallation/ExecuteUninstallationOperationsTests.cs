using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.TaskOptions.Uninstallation;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Husky.Tasks.Uninstallation;
using Moq;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Uninstallation
{
    public class ExecuteUninstallationOperationsTests: BaseHuskyTaskUnitTest<ExecuteUninstallationOperations>
    {
        private readonly Dictionary<UninstallOperationsList.EntryKind, string[]> _uninstallEntriesByEntryKind = new()
        {
            [UninstallOperationsList.EntryKind.File] = new[] { "File1", "File2" },
            [UninstallOperationsList.EntryKind.Directory] = new[] { "Dir1", "Dir2" },
            [UninstallOperationsList.EntryKind.RegistryKey] = new[] { "RegKey1", "RegKey2" },
            [UninstallOperationsList.EntryKind.RegistryValue] = new[] { "Regvalue1", "RegValue2" }
        };

        private Mock<IFileSystemService> _fileSystemServiceMock = null!;
        private Mock<IRegistryService> _registryServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            _fileSystemServiceMock = _fixture.Create<Mock<IFileSystemService>>();
            _registryServiceMock = _fixture.Create<Mock<IRegistryService>>();

            foreach (var entryKind in Enum.GetValues<UninstallOperationsList.EntryKind>())
                UninstallOperationsMock.Setup(s => s.ReadEntries(entryKind)).Returns(_uninstallEntriesByEntryKind[entryKind]);
        }
        [Test]
        [Category("UnitTest")]
        public async ValueTask Execute_uninstallation_operation_removes_alL_entries_from_operations_file()
        {
            // Arrange
            var numFileCalls = 0;
            var numDirCalls = 0;
            var numRegistryKeyCalls = 0;
            var numRegistryValueCalls = 0;

            _fileSystemServiceMock.Setup(s => s.DeleteFile(_uninstallEntriesByEntryKind[UninstallOperationsList.EntryKind.File][numFileCalls++]))
                                  .ca

            _fileSystemServiceMock.Setup(s => s.DeleteDirectory(It.IsAny<string>()));

            // Act
            await Sut.Execute();

            // Assert
            _fileSystemServiceMock.Verify(f => f.);
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Execute_uninstallation_operation_calls_to_remove_files_before_folders()
        {
            // Arrange
            var deleteFilesCalled = false;
            var deleteFolderWasCalledAfterFiles = false;

            _fileSystemServiceMock.Setup(s => s.DeleteFile(It.IsAny<string>())).Callback(() => deleteFilesCalled = true);
            _fileSystemServiceMock.Setup(s => s.DeleteDirectory(It.IsAny<string>()))
                                  .Callback(() => deleteFolderWasCalledAfterFiles = deleteFilesCalled);

            // Act
            await Sut.Execute();

            // Assert
            Assert.True(deleteFilesCalled);
            Assert.True(deleteFolderWasCalledAfterFiles);
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Execute_uninstallation_operation_calls_to_remove_reg_values_before_reg_keys()
        {
            // Arrange
            var deleteValues= false;
            var deleteKeysWasCalledAfterValues = false;
            _registryServiceMock.Setup(s => s.RemoveKeyValue(It.IsAny<string>())).Callback(() => deleteValues = true);
            _registryServiceMock.Setup(s => s.RemoveKey(It.IsAny<string>()))
                                .Callback(() => deleteKeysWasCalledAfterValues = deleteValues);

            // Act
            await Sut.Execute();

            // Assert
            Assert.True(deleteValues);
            Assert.True(deleteKeysWasCalledAfterValues);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) {  }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new ExecuteUninstallationOperationsOptions();
    }
}