using System.IO;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Installer.Infrastructure;
using Serilog;
using Serilog.Core;
using StrongInject;

namespace Husky.Installer.WorkflowExecution
{
    public abstract class WorkflowExecutionBase
    {
        protected HuskyWorkflow Workflow { get; }

        protected HuskyInstallerSettings HuskyInstallerSettings { get; }

        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowExecutionBase));

        protected readonly string UninstallOperationsFile;

        protected WorkflowExecutionBase(HuskyWorkflow workflow, HuskyInstallerSettings huskyInstallerSettings)
        {
            Workflow = workflow;
            HuskyInstallerSettings = huskyInstallerSettings;

            var installPath = Workflow.Configuration.GetConfigurationBlock<ApplicationConfiguration>().InstallDirectory;
            UninstallOperationsFile = Path.Combine(installPath, HuskyConstants.WorkflowDefaults.DefaultUninstallFileName);
        }

        public async ValueTask Execute()
        {
            using var workflowExecutor = new HuskyInstallerContainer().Resolve();

            _logger.Debug("Executing pre-workflow operations");
            await OnBeforeWorkflowExecute();

            var operationsList = await CreateContextUninstallOperationsList();
            await workflowExecutor.Value.Execute(Workflow, HuskyInstallerSettings, operationsList);

            _logger.Debug("Executing post-workflow operations");
            await OnAfterWorkflowExecute();
        }

        protected virtual ValueTask OnBeforeWorkflowExecute() => ValueTask.CompletedTask;

        protected virtual ValueTask OnAfterWorkflowExecute() => ValueTask.CompletedTask;

        protected abstract Task<IUninstallOperationsList> CreateContextUninstallOperationsList();
    }
}