using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Husky.Core.Dependencies;
using Husky.Dependencies.Infrastructure;
using Husky.Dependencies.Services;
using Serilog;
using Serilog.Core;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowDependencyInstaller
    {
        Task InstallDependencies(IEnumerable<HuskyDependency> dependencies);
    }

    public class WorkflowDependencyInstaller: IWorkflowDependencyInstaller
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(WorkflowDependencyInstaller));
        private readonly IDependencyHandlerResolver _dependencyHandlerResolver;
        private readonly IDependencyAcquisitionService _dependencyAcquisitionService;

        public WorkflowDependencyInstaller(IDependencyHandlerResolver dependencyHandlerResolver, IDependencyAcquisitionService dependencyAcquisitionService)
        {
            _dependencyHandlerResolver = dependencyHandlerResolver;
            _dependencyAcquisitionService = dependencyAcquisitionService;
        }

        public async Task InstallDependencies(IEnumerable<HuskyDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                using var ownedDependencyHandler = _dependencyHandlerResolver.Resolve(dependency);
                var dependencyHandler = ownedDependencyHandler.Value;

                if (await dependencyHandler.IsAlreadyInstalled())
                {
                    _logger.Information("Dependency {dependency} is already installed -- skipping", dependency.GetType().Name);
                }
                else
                {
                    if (dependencyHandler.TrySatisfyDependency(out var acquisitionMethod))
                    {
                        _logger.Information("Successfully located a handler for {dependency}, attempting to install", dependency.GetType().Name);
                        await _dependencyAcquisitionService.AcquireDependency(acquisitionMethod);
                        _logger.Debug("Successfully installed dependency {dependency}", dependency.GetType().Name);

                        // Todo: Verify installed (maybe call IsAlreadyInstalled again? :D)
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to acquire dependency {dependency.GetType()}, installation will abort");
                    }
                }
            }
        }
    }
}