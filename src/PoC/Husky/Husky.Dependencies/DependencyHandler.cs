using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;

namespace Husky.Dependencies
{
    public abstract class DependencyHandler<T>: IDependencyHandler<T> where T: HuskyDependency
    {
        public bool TrySatisfyDependency(T dependency, [NotNullWhen(true)] out DependencyAcquisitionMethod<T>? acquisitionMethod)
        {
            acquisitionMethod = GetAvailableDependenciesForCurrentPlatform(dependency).FirstOrDefault(f => f.SatisfiesDependency(dependency));

            if (acquisitionMethod != null)
                return true;

            return false;
        }

        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableWindowsDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableLinuxDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableOsxDependencies(T dependency);

        private IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableDependenciesForCurrentPlatform(T dependency)
            => CurrentPlatform.OS switch
               {
                   OS.Windows => GetAvailableWindowsDependencies(dependency),
                   OS.Linux   => GetAvailableLinuxDependencies(dependency),
                   OS.Osx     => GetAvailableOsxDependencies(dependency),
                   _          => throw new PlatformNotSupportedException($"Cannot install dependencies for unknown OS: {CurrentPlatform.LongDescription}")
               };

        public abstract ValueTask<bool> IsAlreadyInstalled(T dependency);
    }
}