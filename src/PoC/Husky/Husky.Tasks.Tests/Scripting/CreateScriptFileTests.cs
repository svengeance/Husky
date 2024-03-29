﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.TaskOptions.Scripting;
using Husky.Core.Workflow;
using Husky.Tasks.Scripting;
using NUnit.Framework;

namespace Husky.Tasks.Tests.Scripting
{
    public class CreateScriptFileTests: BaseFileSystemTest<CreateScriptFile>
    {
        private string _scriptFileName = "Howler";
        private string _script = "echo Woof!";

        [Test]
        [Category("IntegrationTest")]
        public async Task Create_script_file_creates_file_with_extension_and_exact_contents()
        {
            // Arrange
            // Act
            await Sut.Execute();

            // Assert
            var foundFile = TempDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories).FirstOrDefault(s => s.Name.StartsWith(_scriptFileName));

            FileAssert.Exists(foundFile);

            var fileContents = await File.ReadAllTextAsync(foundFile!.FullName);
            Assert.AreEqual(_script, fileContents);
        }

        protected override void ConfigureHusky(HuskyConfiguration huskyConfiguration) { }

        protected override HuskyTaskConfiguration CreateDefaultTaskConfiguration() => new CreateScriptFileOptions
        {
            Directory = Path.Combine(TempDirectory.FullName, "TestFolder"),
            FileName = _scriptFileName,
            Script = _script
        };
    }
}