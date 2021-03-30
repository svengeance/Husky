using System.Collections.Generic;
using System.Text.Json;
using Husky.Generator.WorkflowParser;
using Husky.Generator.WorkflowParser.YAML;
using NUnit.Framework;

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

        [TestCase(@"
---
  author:
    publisher: Svengeance
    publisherUrl: 'https://sven.ai'
  application:
    name: Husky App
    version: 0.1
    installDirectory: HuskyVariables.Folders.ProgramFiles + ""/HuskyApp""
  clientMachineRequirements:
    supportedOperatingSystems:
      - Windows
      - Linux
    freeSpaceMb: 128
    memoryMb: 1024")]
        [Category("UnitTest")]
        public void Yaml_parser_parses_configuration_blocks(string yaml)
        {
            // Arrange
            var expectedConfigurationBlocks = new Dictionary<string, Dictionary<string, object>>
            {
                ["author"] = new()
                {
                    ["publisher"] = "Svengeance",
                    ["publisherUrl"] = "https://sven.ai",
                },
                ["application"] = new()
                {
                    ["name"] = "Husky App",
                    ["version"] = "0.1",
                    ["installDirectory"] = @"HuskyVariables.Folders.ProgramFiles + ""/HuskyApp""",
                },
                ["clientMachineRequirements"] = new()
                {
                    ["supportedOperatingSystems"] = new[] { "Windows", "Linux" },
                    ["freeSpaceMb"] = "128",
                    ["memoryMb"] = "1024"
                },
            };

            // Act
            var parsedWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            CollectionAssert.AreEquivalent(expectedConfigurationBlocks, parsedWorkflow.ConfigurationBlocks);
        }

        [TestCase(@"
---
  dependencies:
    - DotNet:
        Range: '>=5.0.0'
        FrameworkType: Runtime
        Kind: RuntimeOnly")]
        [Category("UnitTest")]
        public void Yaml_parser_parses_dependencies(string yaml)
        {
            // Arrange
            var expectedDependencyBlocks = new Dictionary<string, Dictionary<string, object>>
            {
                ["DotNet"] = new()
                {
                    ["Range"] = ">=5.0.0",
                    ["FrameworkType"] = "Runtime",
                    ["Kind"] = "RuntimeOnly",
                }
            };

            // Act
            var parsedWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            CollectionAssert.AreEquivalent(expectedDependencyBlocks, parsedWorkflow.Dependencies);
        }

        [TestCase(@"
---
  variables:
    Cat: Kitten
    Dog: Puppy")]
        [Category("UnitTest")]
        public void Yaml_parser_parses_global_variables(string yaml)
        {
            // Arrange
            var expectedVariableBlocks = new Dictionary<string, object>()
            {
                ["Cat"] = "Kitten",
                ["Dog"] = "Puppy"
            };

            // Act
            var parsedWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            CollectionAssert.AreEquivalent(expectedVariableBlocks, parsedWorkflow.Variables);
        }

        [TestCase(@"
---
  stages:
    stage-one:
      jobs:
        job-one:
          steps:
            step-one:
              task: Scripting.ExecuteInlineScript
              with:
                script: echo Hello!
            step-two:
              task: Scripting.ExecuteInlineScript
              with:
                script: echo World!
        job-two:
          steps:
            step-three:
              task: Scripting.ExecuteInlineScript
              with:
                script: echo Puppies!
            step-four:
              task: Scripting.ExecuteInlineScript
              with:
                script: echo Kittens!
    stage-two:
      jobs:
        job-three:
          steps:
            step-five:
              task: Scripting.ExecuteInlineScript
              with:
                script: echo Even more puppies and kittens!")]
        [Category("UnitTest")]
        public void Yaml_parser_parser_stages_jobs_and_steps(string yaml)
        {
            // Arrange
            var expectedWorkflowBlocks = new Dictionary<string, ParsedStage>()
            {
                ["stage-one"] = new()
                {
                    Jobs = new()
                    {
                        ["job-one"] = new ParsedJob
                        {
                            Steps = new()
                            {
                                ["step-one"] = new ParsedStep
                                {
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Hello!"
                                    }
                                },
                                ["step-two"] = new ParsedStep
                                {
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo World!"
                                    }
                                }
                            }
                        },
                        ["job-two"] = new ParsedJob
                        {
                            Steps = new()
                            {
                                ["step-three"] = new ParsedStep
                                {
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Puppies!"
                                    }
                                },
                                ["step-four"] = new ParsedStep
                                {
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Kittens!"
                                    }
                                }
                            }
                        }
                    }
                },
                ["stage-two"] = new()
                {
                    Jobs = new()
                    {
                        ["job-three"] = new ParsedJob
                        {
                            Steps = new()
                            {
                                ["step-five"] = new ParsedStep
                                {
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Even more puppies and kittens!"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var yamlWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            Assert.AreEqual(JsonSerializer.Serialize(expectedWorkflowBlocks), JsonSerializer.Serialize(yamlWorkflow.Stages));
        }

        [TestCase(@"
---
  jobs:
    show-splash:
      tags:
        - Install
      steps:
        show-windows-splash:
          platforms:
            - Windows
          task: Scripting.ExecuteInlineScript
          with:
            script: echo Puppies with jobs!")]
        [Category("UnitTest")]
        public void Yaml_parser_parses_default_stage(string yaml)
        {
            // Arrange
            var expectedWorkflowBlocks = new Dictionary<string, ParsedStage>()
            {
                [GeneratorConstants.Workflow.DefaultStageName] = new()
                {
                    Jobs = new()
                    {
                        ["show-splash"] = new ParsedJob
                        {
                            Tags = new() { "Install" },
                            Steps = new()
                            {
                                ["show-windows-splash"] = new ParsedStep
                                {
                                    Platforms = new() { "Windows" },
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Puppies with jobs!"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var yamlWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            Assert.AreEqual(JsonSerializer.Serialize(expectedWorkflowBlocks), JsonSerializer.Serialize(yamlWorkflow.Stages));
        }

        [TestCase(@"
---
  steps:
    show-windows-splash:
      platforms:
        - Windows
      task: Scripting.ExecuteInlineScript
      with:
        script: echo Kittens with Steps!")]
        [Category("UnitTest")]
        public void Yaml_parser_parses_default_stage_and_job(string yaml)
        {
            // Arrange
            var expectedWorkflowBlocks = new Dictionary<string, ParsedStage>()
            {
                [GeneratorConstants.Workflow.DefaultStageName] = new()
                {
                    Jobs = new()
                    {
                        [GeneratorConstants.Workflow.DefaultJobName] = new ParsedJob
                        {
                            Steps = new()
                            {
                                ["show-windows-splash"] = new ParsedStep
                                {
                                    Platforms = new() { "Windows" },
                                    Task = "Scripting.ExecuteInlineScript",
                                    With = new()
                                    {
                                        ["script"] = "echo Kittens with Steps!"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var yamlWorkflow = _parser.ParseWorkflow(yaml);

            // Assert
            Assert.AreEqual(JsonSerializer.Serialize(expectedWorkflowBlocks), JsonSerializer.Serialize(yamlWorkflow.Stages));
        }
    }
}