using System.IO;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Utilities;
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
        public async Task Create_shortcut_creates_file()
        {
            // Arrange
            // Act
            await Sut.Execute();

            // Assert
            var expectedShortcut = new FileInfo(Path.Combine(_tempDirectory.FullName, "Shortcut.lnk"));
            FileAssert.Exists(expectedShortcut);
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Rollback_create_shortcut_deletes_created_file()
        {
            // Arrange
            // Act
            await Sut.Execute();
            var expectedShortcut = new FileInfo(Path.Combine(_tempDirectory.FullName, "Shortcut.lnk"));
            await Sut.Rollback();

            // Assert
            expectedShortcut.Refresh();
            FileAssert.DoesNotExist(expectedShortcut);
        }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration()
            => new CreateShortcutOptions
            {
                ShortcutName = "Shortcut",
                ShortcutImageFilePath = "test.ico",
                ShortcutLocation = _tempDirectory.FullName,
                Target = Path.Combine(_tempDirectory.FullName, "example.txt")
            };
    }
}