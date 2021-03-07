using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.TaskOptions.Utilities;
using Husky.Core.Workflow;
using Husky.Tasks.Utilities;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Utilities
{
    public class CreateShortcutTests: BaseFileSystemTest<CreateShortcut>
    {
        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        [Test]
        [Category("IntegrationTest")]
        public async Task Create_shortcut_creates_file_and_writes_to_uninstall_operations_list()
        {
            // Arrange
            // Act
            await Sut.Execute();

            // Assert
            var expectedShortcut = new FileInfo(Path.Combine(TempDirectory.FullName, "Shortcut.lnk"));
            FileAssert.Exists(expectedShortcut);

            var operationsListEntry = UninstallOperationsList.ReadEntries(Core.Workflow.Uninstallation.UninstallOperationsList.EntryKind.File).FirstOrDefault();
            Assert.AreEqual(expectedShortcut.FullName, operationsListEntry);
        }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration()
            => new CreateShortcutOptions
            {
                ShortcutName = "Shortcut",
                ShortcutImageFilePath = "test.ico",
                ShortcutLocation = TempDirectory.FullName,
                Target = Path.Combine(TempDirectory.FullName, "example.txt")
            };
    }
}