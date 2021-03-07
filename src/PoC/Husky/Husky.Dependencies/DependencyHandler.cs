using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;

namespace Husky.Dependencies
{
    public abstract class DependencyHandler<T>: IDependencyHandler where T: HuskyDependency
    {
        protected readonly T Dependency;

        protected DependencyHandler(T dependency)
        {
            Dependency = dependency;
        }

        public bool TrySatisfyDependency([NotNullWhen(true)] out IDependencyAcquisitionMethod<HuskyDependency>? acquisitionMethod)
        {
            acquisitionMethod = GetAvailableDependenciesForCurrentPlatform().FirstOrDefault(f => f.SatisfiesDependency(Dependency));

            return acquisitionMethod != null;
        }

        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableWindowsDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableLinuxDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableOsxDependencies(T dependency);

        private IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableDependenciesForCurrentPlatform()
            => CurrentPlatform.OS switch
               {
                   OS.Windows => GetAvailableWindowsDependencies(Dependency),
                   OS.Linux   => GetAvailableLinuxDependencies(Dependency),
                   OS.Osx     => GetAvailableOsxDependencies(Dependency),
                   _          => throw new PlatformNotSupportedException($"Cannot install dependencies for unknown OS: {CurrentPlatform.LongDescription}")
               };

        public abstract ValueTask<bool> IsAlreadyInstalled();
    }
}