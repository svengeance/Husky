using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;

namespace Husky.Installer.Lifecycle
{
    public class HuskyInstaller: LifecycleBase
    {
        public HuskyInstaller(HuskyWorkflow workflow, HuskyInstallerSettings installationSettings): base(workflow, installationSettings) { }

        protected override ValueTask OnBeforeWorkflowExecute()
        {
            Workflow.Stages.Insert(0, GeneratePreInstallationStage());
            Workflow.Stages.Add(GeneratePostInstallationStage());
            return ValueTask.CompletedTask;
        }

        protected override async Task<IUninstallOperationsList> CreateContextUninstallOperationsList()
            => await UninstallOperationsList.CreateOrRead(UninstallOperationsFile);

        private static HuskyStage GeneratePreInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.WorkflowDefaults.PreInstallation.StageName,
                                 stage => stage.AddJob(HuskyConstants.WorkflowDefaults.PreInstallation.JobName,
                                     job => job.AddStep<VerifyMachineMeetsRequirementsOptions>(HuskyConstants.WorkflowDefaults.PreInstallation.Steps.VerifyClientMachineMeetsRequirements,
                                         new HuskyStepConfiguration(CurrentPlatform.OS, HuskyConstants.StepTags.Install))))
                            .Build()
                            .Stages[0];

        private static HuskyStage GeneratePostInstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.WorkflowDefaults.PostInstallation.StageName,
                                 stage => stage.AddJob(HuskyConstants.WorkflowDefaults.PostInstallation.JobName,
                                     job => job.AddStep<PostInstallationApplicationRegistrationOptions>(HuskyConstants.WorkflowDefaults.PostInstallation.Steps.PostInstallationApplicationRegistration)))
                            .Build()
                            .Stages[0];
    }
}