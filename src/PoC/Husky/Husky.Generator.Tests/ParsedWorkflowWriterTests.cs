using System;
using System.Collections.Generic;
using Husky.Generator.WorkflowParser;
using NUnit.Framework;

namespace Husky.Generator.Tests
{
    public class ParsedWorkflowWriterTests
    {
        private const string ShellBegin = @"namespace Husky.Generated
{
    public static class Workflow
    {
        public static global::Husky.Core.Workflow.HuskyWorkflow Create()
        {
            return global::Husky.Core.Workflow.HuskyWorkflow.Create()";

        private const string ShellEnd = @"
            .Build();
        }
    }
}
";

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_outputs_shell_with_an_empty_workflow()
        {
            // Arrange
            var workflow = new ParsedWorkflow();

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(WrapWithShell(string.Empty), result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_configuration_blocks()
        {
            // Arrange
            var workflow = new ParsedWorkflow
            {
                ConfigurationBlocks = new()
                {
                    ["Puppy"] = new()
                    {
                        ["Fur"] = "Soft",
                        ["Age"] = 2,
                    },
                    ["Kitten"] = new()
                    {
                        ["Noises"] = new[] { "Eep!", "Mew" }
                    }
                }
            };

            var expectedResult = WrapWithShell(
                @"
            .Configure<global::Husky.Core.HuskyConfiguration.PuppyConfiguration>(Puppy =>
            {
                Puppy.Fur = @""Soft"";
                Puppy.Age = 2;
            })
            .Configure<global::Husky.Core.HuskyConfiguration.KittenConfiguration>(Kitten =>
            {
                Kitten.Noises = new[] { ""Eep!"", ""Mew"" };
            })");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_dependencies()
        {
            // Arrange
            var workflow = new ParsedWorkflow
            {
                Dependencies = new()
                {
                    ["KittenFactory"] = new()
                    {
                        ["Version"] = 12.0,
                        ["FactoryKind"] = "FreeRangeOrganic"
                    },
                    ["PuppyFactory"] = new()
                    {
                        ["Breed"] = "Husky"
                    }
                }
            };

            var expectedResult = WrapWithShell(
                @"
            .AddDependency(new global::Husky.Core.Dependencies.KittenFactory(Version: 12, FactoryKind: Husky.Core.Dependencies.KittenFactory.Factory.FreeRangeOrganic))
            .AddDependency(new global::Husky.Core.Dependencies.PuppyFactory(Breed: @""Husky""))");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_global_variables()
        {
            // Arrange
            var workflow = new ParsedWorkflow
            {
                Variables = new()
                {
                    ["Cat"] = "{Kitten}/Kittens",
                    ["Dog"] = "{Puppy}/Puppies",
                    ["NumberOfPets"] = 10,
                    ["ReallyLikesPets"] = true,
                }
            };

            var expectedResult = WrapWithShell(@"
            .AddGlobalVariable(""Cat"", @""{Kitten}/Kittens"")
            .AddGlobalVariable(""Dog"", @""{Puppy}/Puppies"")
            .AddGlobalVariable(""NumberOfPets"", 10)
            .AddGlobalVariable(""ReallyLikesPets"", true)");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_workflow_with_stage_job_and_steps()
        {
            // Arrange
            var workflow = new ParsedWorkflow
            {
                Stages = new Dictionary<string, ParsedStage>()
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
                }
            };

            var expectedResult = WrapWithShell(@"
            .AddStage(
                ""stage-one"",
                stage => stage
                .AddJob(
                    ""job-one"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""step-one"",
                        task =>
                        {
                            task.Script = @""echo Hello!"";
                        })
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""step-two"",
                        task =>
                        {
                            task.Script = @""echo World!"";
                        })
                    )
                .AddJob(
                    ""job-two"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""step-three"",
                        task =>
                        {
                            task.Script = @""echo Puppies!"";
                        })
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""step-four"",
                        task =>
                        {
                            task.Script = @""echo Kittens!"";
                        })
                    )
                )
            .AddStage(
                ""stage-two"",
                stage => stage
                .AddJob(
                    ""job-three"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""step-five"",
                        task =>
                        {
                            task.Script = @""echo Even more puppies and kittens!"";
                        })
                    )
                )");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_workflow_with_default_stage()
        {
            // Arrange
            var workflow = new ParsedWorkflow()
            {
                Stages = new Dictionary<string, ParsedStage>()
                {
                    [GeneratorConstants.Workflow.DefaultStageName] = new()
                    {
                        Jobs = new()
                        {
                            ["talk-about-puppies"] = new ParsedJob
                            {
                                Os = "AniOs",
                                Tags = new() { "Install" },
                                Steps = new()
                                {
                                    ["describe-puppies"] = new ParsedStep
                                    {
                                        Os = "Windows",
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
                }
            };

            var expectedResult = WrapWithShell(@"
            .WithDefaultStage(
                stage => stage
                .AddJob(
                    ""talk-about-puppies"",
                    job => job
                    .SetDefaultStepConfiguration(new(global::Husky.Core.Enums.OS.AniOs, @""Install""))
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""describe-puppies"",
                        task =>
                        {
                            task.Script = @""echo Puppies with jobs!"";
                        }
                        ,new(global::Husky.Core.Enums.OS.Windows))
                    )
                )");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [Category("UnitTest")]
        public void Workflow_writer_writes_workflow_with_default_stage_and_job()
        {
            // Arrange
            var workflow = new ParsedWorkflow
            {
                Stages = new Dictionary<string, ParsedStage>()
                {
                    [GeneratorConstants.Workflow.DefaultStageName] = new()
                    {
                        Jobs = new()
                        {
                            [GeneratorConstants.Workflow.DefaultJobName] = new ParsedJob
                            {
                                Steps = new()
                                {
                                    ["describe-kittens"] = new ParsedStep
                                    {
                                        Os = "Windows",
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
                }
            };

            var expectedResult = WrapWithShell(@"
            .WithDefaultStage(
                stage => stage
                .WithDefaultJob(
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""describe-kittens"",
                        task =>
                        {
                            task.Script = @""echo Kittens with Steps!"";
                        }
                        ,new(global::Husky.Core.Enums.OS.Windows))
                    )
                )");

            // Act
            var result = ParseWorkflow(workflow);

            // Assert
            Console.WriteLine(result);
            Assert.AreEqual(expectedResult, result);
        }

        private string WrapWithShell(string s)
            => ShellBegin + s + ShellEnd;

        private string ParseWorkflow(ParsedWorkflow workflow) => ParsedWorkflowWriter.Write(workflow);
    }
}