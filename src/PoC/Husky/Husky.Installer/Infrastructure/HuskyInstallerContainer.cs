using Husky.Core.Infrastructure;
using Husky.Dependencies.Infrastructure;
using Husky.Installer.WorkflowExecution;
using Husky.Services.Infrastructure;
using Husky.Tasks.Infrastructure;
using StrongInject;

namespace Husky.Installer.Infrastructure
{
    [RegisterModule(typeof(HuskyCoreModule))]
    [RegisterModule(typeof(HuskyDependenciesModule))]
    [RegisterModule(typeof(HuskyInstallerModule))]
    [RegisterModule(typeof(HuskyServicesModule))]
    [RegisterModule(typeof(HuskyTasksModule))]
    public partial class HuskyInstallerContainer: IContainer<IWorkflowExecutor> { }
}