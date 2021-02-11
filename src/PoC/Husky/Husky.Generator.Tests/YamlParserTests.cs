using Husky.Generator.WorkflowParser.YAML;
using NUnit.Framework;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
  name: Husky App # Initial app metadata is set at the top
  version: 0.1
  publisher: Svengeance
  dependencies: # Dependencies that Husky can't track are explicitly stated
      - DotNetCore>=5.0
  variables: # Global constant values the user can set up top
      installDir: $folders.ProgramFiles + /HuskyApp

  jobs: # A single job can exist without an explicitly declared stage
      show-splash:
          steps:
              - name: Display Unix Splash Screen
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

              - name: Display Windows Splash Screen
                platforms:
                  - Windows
                task: Scripting.ExecuteInlineScript
                with:
                  script: | # Windows (cmd) specific script
                    clear &&
                    echo Welcome to Husky-App! &&
                    pause
                
      extract-husky-app:
          steps:
              - name: Extract App Files
                task: Resources.ExtractBundledResource
                with:
                  resources: dist/program/**/* # standard file globbing pattern
                  targetDirectory: $variables.installDir
                  clean: true # Clean the destination directory 

      create-shortcut:
          steps:
              - name: Create Launch Shortcut
                task: Scripting.CreateScriptFile # Create a platform-independent script file (.sh on linux, .bat on windows)
                with:
                  directory: $variables.installDir
                  fileName: launch
                  script: dotnet Huskyapp.dll
                output: # assign output variables FROM the step into inter-job variables
                  app-launch-file: createdFileName

              - name: Create Desktop Shortcut
                task: Utilities.CreateShortcut # Create a platform-independent shortcut 
                with:
                  target: $jobs.create-shortcut.variables.app-launch-file # Variables are scoped at the job level")]
        [Category("UnitTest")]
        public void Test1(string yaml)
        {
            // Arrange

            // Act
            var yamlWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            Assert.IsNotNull(yamlWorkflow);
        }
    }
}