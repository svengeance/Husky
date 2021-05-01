using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow;
using Husky.Core.Workflow.Uninstallation;
using Husky.Tasks;
using Serilog;
using Serilog.Context;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowExecutor
    {
        ValueTask Execute(HuskyWorkflow huskyWorkflow, HuskyInstallerSettings installerSettings, IUninstallOperationsList uninstallOperationsList);
    }

    public class WorkflowExecutor: IWorkflowExecutor
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowExecutor));
        private readonly IWorkflowStageExecutor _stageExecutor;
        private readonly IWorkflowValidator _workflowValidator;
        private readonly IWorkflowDependencyInstaller _dependencyInstaller;

        public WorkflowExecutor(IWorkflowStageExecutor stageExecutor, IWorkflowValidator workflowValidator, IWorkflowDependencyInstaller dependencyInstaller)
        {
            _stageExecutor = stageExecutor;
            _workflowValidator = workflowValidator;
            _dependencyInstaller = dependencyInstaller;
        }

        public async ValueTask Execute(HuskyWorkflow huskyWorkflow, HuskyInstallerSettings installerSettings, IUninstallOperationsList uninstallOperationsList)
        {
            var combinedVariables = _workflowValidator.ValidateWorkflow(huskyWorkflow);

            var huskyContext = new HuskyContext(uninstallOperationsList, Assembly.GetEntryAssembly()!, installerSettings.TagToExecute)
            {
                Variables = new Dictionary<string, object>(combinedVariables, StringComparer.InvariantCultureIgnoreCase)
            };

            if (installerSettings.TagToExecute == HuskyConstants.StepTags.Install)
                await _dependencyInstaller.InstallDependencies(huskyWorkflow.Dependencies);

            foreach (var stageToExecute in huskyWorkflow.Stages)
            {
                using var stageScope = LogContext.PushProperty("Stage", stageToExecute.Name + ".");
                _logger.Information("Executing stage {stage}", stageToExecute.Name);
                await _stageExecutor.ExecuteStage(stageToExecute, huskyContext);
            }

            _logger.Information("Husky has successfully executed {tag}", installerSettings.TagToExecute);
        }
    }
}