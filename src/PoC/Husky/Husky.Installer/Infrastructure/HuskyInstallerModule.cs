using Husky.Installer.WorkflowExecution;
using StrongInject;

namespace Husky.Installer.Infrastructure
{
    [Register(typeof(WorkflowDependencyInstaller), typeof(IWorkflowDependencyInstaller))]
    [Register(typeof(WorkflowExecutor), typeof(IWorkflowExecutor))]
    [Register(typeof(WorkflowJobExecutor), typeof(IWorkflowJobExecutor))]
    [Register(typeof(WorkflowStageExecutor), typeof(IWorkflowStageExecutor))]
    [Register(typeof(WorkflowStepExecutor), typeof(IWorkflowStepExecutor))]
    [Register(typeof(WorkflowTaskExecutor), typeof(IWorkflowTaskExecutor))]
    [Register(typeof(WorkflowValidator), typeof(IWorkflowValidator))]
    public class HuskyInstallerModule { }
}