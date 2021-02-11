using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Moq;
using NUnit.Framework;

namespace Husky.Installer.Tests.InstallerTests
{
    public class HuskyInstallerTests: BaseInstallerTest
    {
        [Test]
        [Category("UnitTest")]
        public void Created_workflow_has_preinstallation_stage_and_job()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(HuskyConstants.Workflows.PreInstallation.DefaultStageName, Workflow.Stages[0].Name);
            Assert.AreEqual(HuskyConstants.Workflows.PreInstallation.DefaultJobName, Workflow.Stages[0].Jobs[0].Name);
        }

        [Test]
        [Category("UnitTest")]
        public void Created_workflow_has_postinstallation_stage_and_job()
        {
            // Arrange
            // Act
            // Assert
            Assert.AreEqual(HuskyConstants.Workflows.PostInstallation.DefaultStageName, Workflow.Stages[^1].Name);
            Assert.AreEqual(HuskyConstants.Workflows.PostInstallation.DefaultJobName, Workflow.Stages[^1].Jobs[0].Name);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Installer_validates_workflow()
        {
            // Arrange
            var settings = new HuskyInstallerSettings { ResolveModulesFromAssemblies = new[] { Assembly.GetExecutingAssembly() } };
            var installer = new HuskyInstaller(Workflow, settings);

            // Act
            await installer.Execute();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions) Workflow.Stages.First(f => f.Name != HuskyConstants.Workflows.PreInstallation.DefaultStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.True(testTaskOptions.HasValidated);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Installer_replaces_variables_on_task_configuration()
        {
            // Arrange
            var expectedTitle = "Test - 4";

            // Act
            await Installer.Execute();

            // Assert
            var testTaskOptions = (TestHuskyTaskOptions)Workflow.Stages.First(f => f.Name != HuskyConstants.Workflows.PreInstallation.DefaultStageName).Jobs[0].Steps[0].HuskyTaskConfiguration;
            Assert.AreEqual(expectedTitle, testTaskOptions.Title);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Installer_executes_only_tasks_with_correct_tag()
        {
            // Arrange
            var tasksToExecute = HuskyConstants.Workflows.StepTags.Repair;
            var additionalWorkflow = HuskyWorkflow.Create()
                                                  .WithDefaultStageAndJob(
                                                       job => job.AddStep<TestHuskyTaskOptions>("RepairStep", ConfigureTestTaskOptions,
                                                           new HuskyStepConfiguration(CurrentPlatform.OS) { Tags = new[] { HuskyConstants.Workflows.StepTags.Repair } }))
                                                  .Build();

            var repairStep = additionalWorkflow.Stages[0].Jobs[0].Steps[0];
            Workflow.Stages[0].Jobs[0].Steps.Add(repairStep);

            // Act
            await Installer.Execute();

            // Assert
            foreach (var step in Workflow.Stages.SelectMany(s => s.Jobs).SelectMany(s => s.Steps))
            {
                if (step.HuskyStepConfiguration.Tags.Contains(InstallerSettings.TagToExecute))
                    Assert.AreEqual(ExecutionStatus.Completed, step.ExecutionInformation.ExecutionStatus);
                else
                    Assert.AreEqual(ExecutionStatus.NotStarted, step.ExecutionInformation.ExecutionStatus);
            }
        }

        protected override void ConfigureTestTaskOptions(TestHuskyTaskOptions options)
        {
            options.Title = "Test - {random.RandomNumber}";
        }
    }
}