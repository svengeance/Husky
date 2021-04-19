using System.IO;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Installer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Husky.Installer.WorkflowExecution
{
    public abstract class WorkflowExecutionBase
    {
        protected HuskyWorkflow Workflow { get; }

        protected HuskyInstallerSettings HuskyInstallerSettings { get; }

        protected ILogger Logger { get; private set; }

        protected readonly string UninstallOperationsFile;
        protected readonly ILoggerFactory LoggerFactory;

        private readonly IServiceScopeFactory _scopeFactory;

        protected WorkflowExecutionBase(HuskyWorkflow workflow, HuskyInstallerSettings huskyInstallerSettings)
        {
            Workflow = workflow;
            HuskyInstallerSettings = huskyInstallerSettings;

            var serviceProvider = new ServiceCollection().AddHuskyInstaller(HuskyInstallerSettings, Workflow.Configuration);
            
            _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            Logger = LoggerFactory.CreateLogger(this.GetType().FullName);
            Logger.LogDebug("Constructed ServiceProvider");

            var installPath = Workflow.Configuration.GetConfigurationBlock<ApplicationConfiguration>().InstallDirectory;
            UninstallOperationsFile = Path.Combine(installPath, HuskyConstants.WorkflowDefaults.DefaultUninstallFileName);
        }

        public async ValueTask Execute()
        {
            using var scope = _scopeFactory.CreateScope();
            var workflowExecutor = scope.ServiceProvider.GetRequiredService<IWorkflowExecutor>();

            await OnBeforeWorkflowExecute();

            var operationsList = await CreateContextUninstallOperationsList();
            await workflowExecutor.Execute(Workflow, HuskyInstallerSettings, operationsList);

            await OnAfterWorkflowExecute();
        }

        protected virtual ValueTask OnBeforeWorkflowExecute() => ValueTask.CompletedTask;

        protected virtual ValueTask OnAfterWorkflowExecute() => ValueTask.CompletedTask;

        protected abstract Task<IUninstallOperationsList> CreateContextUninstallOperationsList();
    }
}