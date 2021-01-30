using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;
using Husky.Tasks.Scripting;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Scripting
{
    public class CreateScriptFileTests: BaseFileSystemTest<CreateScriptFile>
    {
        private string _scriptFileName = "TestScript";
        private string _script = "echo Woof!";

        [Test]
        [Category("IntegrationTest")]
        public async Task Create_script_file_creates_file_with_extension_and_exact_contents()
        {
            // Arrange
            // Act
            await Sut.Execute();

            // Assert
            var foundFile = TempDirectory.EnumerateFiles().FirstOrDefault(s => s.Name.StartsWith(_scriptFileName));

            Assert.NotNull(foundFile);
            FileAssert.Exists(foundFile);

            var fileContents = await File.ReadAllTextAsync(foundFile!.FullName);
            Assert.AreEqual(_script, fileContents);
        }

        [Test]
        [Category("IntegrationTest")]
        public async Task Rollback_create_script_file_deletes_file()
        {
            // Arrange
            // Act
            await Sut.Execute();
            var foundFile = TempDirectory.EnumerateFiles().First(s => s.Name.StartsWith(_scriptFileName));
            await Sut.Rollback();

            // Assert
            foundFile.Refresh();
            FileAssert.DoesNotExist(foundFile);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new CreateScriptFileOptions
        {
            Directory = TempDirectory.FullName,
            FileName = "TestScript",
            Script = _script
        };
    }
}