using System.Threading.Tasks;
using AutoFixture;
using Husky.Core.TaskOptions.Scripting;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Husky.Tasks.Scripting;
using Moq;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Scripting
{
    public class CreateScriptFileUnitTests: BaseHuskyTaskUnitTest<CreateScriptFile>
    {
        private string _scriptDirectory = "Puppies";
        private string _scriptFileName = "Puppies";
        private string _script = "echo Woof!";

        [Test]
        [Category("UnitTest")]
        public async Task Create_script_file_creates_file_with_extension_and_exact_contents()
        {
            // Arrange
            var scriptCreatedMockReturn = "A puppy!";
            var fileSystemServiceMock = Fixture.Create<Mock<IFileSystemService>>();
            fileSystemServiceMock.Setup(s => s.CreateScriptFile(_scriptDirectory, _scriptFileName, _script)).ReturnsAsync(scriptCreatedMockReturn);

            // Act
            await Sut.Execute();

            // Assert
            UninstallOperationsMock.Verify(f => f.AddEntry(UninstallOperationsList.EntryKind.File, scriptCreatedMockReturn), Times.Once);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new CreateScriptFileOptions
        {
            Directory = _scriptDirectory,
            FileName = _scriptFileName,
            Script = _script
        };
    }
}