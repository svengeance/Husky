using Husky.Core.TaskOptions.Scripting;
using Husky.Core.Workflow;
using NUnit.Framework;

namespace Husky.Core.Tests.HuskyWorkflowTests
{
    public class HuskyWorkflowTests
    {
        [Test]
        [Category("UnitTest")]
        public void Reversing_workflow_reverses_stages_jobs_and_steps()
        {
            // Arrange
            var workflow = HuskyWorkflow.Create()
                                        .AddStage("s1",
                                             stage1 => stage1.AddJob("s1j1",
                                                                  job1 => job1.AddStep<ExecuteInlineScriptOptions>("s1j1s1")
                                                                              .AddStep<ExecuteInlineScriptOptions>("s1j1s2")
                                                                              .AddStep<ExecuteInlineScriptOptions>("s1j1s3"))
                                                             .AddJob("s1j2",
                                                                  job2 => job2.AddStep<ExecuteInlineScriptOptions>("s1j2s1")
                                                                              .AddStep<ExecuteInlineScriptOptions>("s1j2s2")))
                                        .AddStage("s2",
                                             stage2 => stage2.AddJob("s2j1",
                                                                  job1 => job1.AddStep<ExecuteInlineScriptOptions>("s2j1s1")
                                                                              .AddStep<ExecuteInlineScriptOptions>("s2j1s2")))
                                        .Build();


            // Act
            workflow.Reverse();

            // Assert
            Assert.AreEqual("s2", workflow.Stages[0].Name);
            Assert.AreEqual("s2j1s2", workflow.Stages[0].Jobs[0].Steps[0].Name);
            Assert.AreEqual("s2j1s1", workflow.Stages[0].Jobs[0].Steps[1].Name);

            Assert.AreEqual("s1", workflow.Stages[1].Name);
            Assert.AreEqual("s1j2s2", workflow.Stages[1].Jobs[0].Steps[0].Name);
            Assert.AreEqual("s1j2s1", workflow.Stages[1].Jobs[0].Steps[1].Name);
            Assert.AreEqual("s1j1s3", workflow.Stages[1].Jobs[1].Steps[0].Name);
            Assert.AreEqual("s1j1s2", workflow.Stages[1].Jobs[1].Steps[1].Name);
            Assert.AreEqual("s1j1s1", workflow.Stages[1].Jobs[1].Steps[2].Name);
        }
    }
}