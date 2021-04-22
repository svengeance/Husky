using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Husky.Core.HuskyConfiguration;
using Husky.Dependencies;
using Husky.Dependencies.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Installer.WorkflowExecution
{
    public interface IWorkflowDependencyInstaller
    {
        Task InstallDependencies(IEnumerable<HuskyDependency> dependencies);
    }

    public class WorkflowDependencyInstaller: IWorkflowDependencyInstaller
    {
        private readonly ILogger<WorkflowDependencyInstaller> _logger;
        private readonly IDependencyHandlerResolver _dependencyHandlerResolver;
        private readonly IDependencyAcquisitionService _dependencyAcquisitionService;

        public WorkflowDependencyInstaller(ILogger<WorkflowDependencyInstaller> logger, IDependencyHandlerResolver dependencyHandlerResolver, IDependencyAcquisitionService dependencyAcquisitionService)
        {
            _logger = logger;
            _dependencyHandlerResolver = dependencyHandlerResolver;
            _dependencyAcquisitionService = dependencyAcquisitionService;
        }

        public async Task InstallDependencies(IEnumerable<HuskyDependency> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                var dependencyHandler = _dependencyHandlerResolver.Resolve(dependency);
                if (await dependencyHandler.IsAlreadyInstalled())
                {
                    _logger.LogInformation("Dependency {dependency} is already installed -- skipping", dependency.GetType().Name);
                }
                else
                {
                    if (dependencyHandler.TrySatisfyDependency(out var acquisitionMethod))
                    {
                        _logger.LogInformation("Successfully located a handler for {dependency}, attempting to install", dependency.GetType().Name);
                        await _dependencyAcquisitionService.AcquireDependency(acquisitionMethod);
                        _logger.LogDebug("Successfully installed dependency {dependency}", dependency.GetType().Name);

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