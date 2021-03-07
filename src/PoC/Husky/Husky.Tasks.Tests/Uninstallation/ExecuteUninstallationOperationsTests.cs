using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.TaskOptions.Uninstallation;
using Husky.Core.Workflow;
using Husky.Services;
using Husky.Tasks.Uninstallation;
using Moq;
using NUnit.Framework;

using EntryKind = Husky.Core.Workflow.Uninstallation.UninstallOperationsList.EntryKind;

namespace Husky.Tasks.Tests.Uninstallation
{
    public class ExecuteUninstallationOperationsTests: BaseHuskyTaskUnitTest<ExecuteUninstallationOperations>
    {
        private readonly Dictionary<EntryKind, string[]> _uninstallEntriesByEntryKind = new()
        {
            [EntryKind.File] = new[] { "File1", "File2" },
            [EntryKind.Directory] = new[] { "Dir1", "Dir2" },
            [EntryKind.RegistryKey] = new[] { "RegKey1", "RegKey2" },
            [EntryKind.RegistryValue] = new[] { "Regvalue1", "RegValue2" }
        };

        private Mock<IFileSystemService> _fileSystemServiceMock = null!;
        private Mock<IRegistryService> _registryServiceMock = null!;

        [SetUp]
        public void Setup()
        {
            _fileSystemServiceMock = Fixture.Create<Mock<IFileSystemService>>();
            _registryServiceMock = Fixture.Create<Mock<IRegistryService>>();

            foreach (var entryKind in Enum.GetValues<EntryKind>())
                UninstallOperationsMock.Setup(s => s.ReadEntries(entryKind)).Returns(_uninstallEntriesByEntryKind[entryKind]);
        }
        [Test]
        [Category("UnitTest")]
        public async ValueTask Execute_uninstallation_operation_removes_all_entries_from_operations_file()
        {
            // Arrange
            var fileCalls = new List<string>();
            var dirCalls = new List<string>();
            var regValueCalls = new List<string>();
            var regKeyCalls = new List<string>();

            _fileSystemServiceMock.Setup(s => s.DeleteFile(Capture.In(fileCalls)));
            _fileSystemServiceMock.Setup(s => s.DeleteDirectory(Capture.In(dirCalls), true));
            _registryServiceMock.Setup(s => s.RemoveKeyValue(Capture.In(regValueCalls)));
            _registryServiceMock.Setup(s => s.RemoveKey(Capture.In(regKeyCalls)));

            // Act
            await Sut.Execute();

            // Assert
            CollectionAssert.AreEqual(_uninstallEntriesByEntryKind[EntryKind.File], fileCalls);
            CollectionAssert.AreEqual(_uninstallEntriesByEntryKind[EntryKind.Directory], dirCalls);
            CollectionAssert.AreEqual(_uninstallEntriesByEntryKind[EntryKind.RegistryValue], regValueCalls);
            CollectionAssert.AreEqual(_uninstallEntriesByEntryKind[EntryKind.RegistryKey], regKeyCalls);
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Execute_uninstallation_operation_calls_to_remove_files_before_folders()
        {
            // Arrange
            var deleteFilesCalled = false;
            var deleteFolderWasCalledAfterFiles = false;

            _fileSystemServiceMock.Setup(s => s.DeleteFile(It.IsAny<string>())).Callback(() => deleteFilesCalled = true);
            _fileSystemServiceMock.Setup(s => s.DeleteDirectory(It.IsAny<string>(), true))
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