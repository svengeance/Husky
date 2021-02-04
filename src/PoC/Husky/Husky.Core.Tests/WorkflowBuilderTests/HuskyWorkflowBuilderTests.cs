using System.Linq;
using Husky.Core.Enums;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Core.Tests.WorkflowBuilderTests
{
    public class HuskyWorkflowBuilderTests
    {
        [Test]
        [Category("UnitTest")]
        public void Setting_default_step_configuration_on_job_configures_all_subsequent_steps()
        {
            // Arrange
            var builder = HuskyWorkflow.Create()
                                       .WithDefaultStage(
                                            stage => stage.AddJob("test-job",
                                                job => job.SetDefaultStepConfiguration(new HuskyStepConfiguration(OS.Linux)
                                                           {
                                                               Tags = new[] { "My-Custom-Tag" }
                                                           })
                                                          .AddStep<ExecuteInlineScriptOptions>("test-step", o => o.Script = "echo Hello")
                                                          .AddStep<ExecuteInlineScriptOptions>("test-step", o => o.Script = "echo World!")));

            // Act
            var workflow = builder.Build();

            // Assert
            Assert.True(workflow.Stages[0].Jobs[0].Steps.All(a => a.HuskyStepConfiguration.Os == OS.Linux));
            Assert.True(workflow.Stages[0].Jobs[0].Steps.All(a => a.HuskyStepConfiguration.Tags[0] == "My-Custom-Tag"));
        }

        [Test]
        [Category("UnitTest")]
        public void Setting_step_configuration_overrides_default_step_configuration()
        {
            // Arrange
            var windowsConfiguration = new HuskyStepConfiguration(OS.Windows) { Tags = new[] { "Windows-Steps" } };
            var builder = HuskyWorkflow.Create()
                                       .WithDefaultStage(
                                            stage => stage.AddJob("test-job",
                                                job => job.SetDefaultStepConfiguration(new HuskyStepConfiguration(OS.Linux)
                                                           {
                                                               Tags = new[] { "My-Custom-Tag" }
                                                           })
                                                          .AddStep<ExecuteInlineScriptOptions>("test-step", o => o.Script = "echo Hello")
                                                          .AddStep<ExecuteInlineScriptOptions>("test-step", o => o.Script = "echo Hello Windows!", windowsConfiguration)
                                                          .AddStep<ExecuteInlineScriptOptions>("test-step", o => o.Script = "echo World!")));

            // Act
            var workflow = builder.Build();

            // Assert
            Assert.AreEqual(workflow.Stages[0].Jobs[0].Steps[1].HuskyStepConfiguration.Os, windowsConfiguration.Os);
            Assert.AreEqual(workflow.Stages[0].Jobs[0].Steps[1].HuskyStepConfiguration.Tags, windowsConfiguration.Tags);
        }
    }
}