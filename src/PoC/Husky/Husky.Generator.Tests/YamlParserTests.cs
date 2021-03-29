using System;
using System.Text.Json;
using Husky.Generator.WorkflowParser.YAML;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Husky.Generator.Tests
{
    public class YamlParserTests
    {
        private YamlWorkflowParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new YamlWorkflowParser();
        }

        [TestCase(@"---
  author:
    publisher: Svengeance
    publisherUrl: https://sven.ai
  application:
    name: Husky App # Initial app metadata is set at the top
    version: 0.1
    installDirectory: HuskyVariables.Folders.ProgramFiles + ""/HuskyApp"";
  clientMachineRequirements:
    supportedOperatingSystems: [Windows, Linux]
    freeSpaceMb: 128
    memoryMb: 1024
  dependencies: # Dependencies that Husky can track are explicitly stated
      - DotNet:
          Range: '>=5.0.0'
          FrameworkType: Runtime
          Kind: RuntimeOnly

  jobs: # A single job can exist without an explicitly declared stage
      show-splash:
        tags: [Install]
        steps:
              show-unix-splash:
                platforms:
                  - Linux
                  - OSX
                  - FreeBSD
                task: Scripting.ExecuteInlineScript
                with:
                  script: | # Unix-specific script
                    cls &&
                    echo Welcome to Husky-App! &&
                    read -n 1 -r -s -p $'Press any key to continue installation...\n'

              show-windows-splash:
                platforms:
                  - Windows
                task: Scripting.ExecuteInlineScript
                with:
                  script: | # Windows (cmd) specific script
                    cls &&
                    echo Welcome to Husky-App! &&
                    pause


      extract-husky-app:
          steps:
              extract-files:
                task: Resources.ExtractBundledResource
                with:
                  resources: '**/*' # standard file globbing pattern
                  targetDirectory: '{Folders.ProgramFiles}/HuskyApp'

      create-launch-file:
          steps:
              create-launch-script:
                task: Scripting.CreateScriptFile # Create a platform-independent script file (.sh on linux, .bat on windows)
                with:
                  directory: '{Folders.ProgramFiles}/HuskyApp'
                  fileName: launch
                  script: dotnet ""{Folders.ProgramFiles}/HuskyApp/HuskyApp.dll""

              create-shortcut:
                task: Utilities.CreateShortcut # Create a platform-independent shortcut 
                with:
                  shortcutLocation: '{folders.Desktop}'
                  shortcutName: 'HuskyApp'
                  target: '{create-launch-file.create-launch-script.createdFileName}' # Retrieve variable from prior step")]
        [Category("UnitTest")]
        public void Test1(string yaml)
        {
            // Arrange

            // Act
            var yamlWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            Assert.IsNotNull(yamlWorkflow);
            JsonSerializer.Create().Serialize(Console.Out, yamlWorkflow);
        }
    }
}