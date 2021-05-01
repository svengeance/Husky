using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Uninstallation;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Installer.WorkflowExecution;

namespace Husky.Installer
{
    public class HuskyUninstaller: WorkflowExecutionBase
    {
        public HuskyUninstaller(HuskyWorkflow workflow, HuskyInstallerSettings huskyInstallerSettings): base(workflow, huskyInstallerSettings) { }

        protected override ValueTask OnBeforeWorkflowExecute()
        {
            Workflow.Stages.Add(GeneratePostUninstallationStage());
            return ValueTask.CompletedTask;
        }

        protected override async Task<IUninstallOperationsList> CreateContextUninstallOperationsList()
            => new ReadonlyUninstallOperationsList(await UninstallOperationsList.CreateOrRead(UninstallOperationsFile));

        private static HuskyStage GeneratePostUninstallationStage()
            => HuskyWorkflow.Create()
                            .AddStage(HuskyConstants.WorkflowDefaults.PreUninstallation.StageName,
                                 stage => stage.AddJob(HuskyConstants.WorkflowDefaults.PreUninstallation.JobName,
                                     job => job.AddStep<ExecuteUninstallationOperationsOptions>(HuskyConstants.WorkflowDefaults.PreUninstallation.Steps.ExecuteUninstallationOperations,
                                         new HuskyStepConfiguration(CurrentPlatform.OS, HuskyConstants.StepTags.Uninstall))))
                            .Build()
                            .Stages[0];
    }
}