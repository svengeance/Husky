using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;

namespace Husky.Dependencies
{
    public abstract class DependencyHandler<T>: IDependencyHandler where T: HuskyDependency
    {
        protected T? Dependency { get; private set; }

        public void LoadDependency(T dependency) =>
            Dependency = (Dependency is null
                ? dependency
                : throw new ArgumentException("Dependency may only be set once."));

        public bool TrySatisfyDependency([NotNullWhen(true)] out IDependencyAcquisitionMethod<HuskyDependency>? acquisitionMethod)
        {
            if (Dependency is null)
                throw DependencyUnsetException;

            acquisitionMethod = GetAvailableDependenciesForCurrentPlatform().FirstOrDefault(f => f.SatisfiesDependency(Dependency));

            return acquisitionMethod != null;
        }

        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableWindowsDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableLinuxDependencies(T dependency);
        protected abstract IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableOsxDependencies(T dependency);

        private IEnumerable<DependencyAcquisitionMethod<T>> GetAvailableDependenciesForCurrentPlatform()
            => CurrentPlatform.OS switch
               {
                   _ when Dependency is null => throw DependencyUnsetException,
                   OS.Windows                => GetAvailableWindowsDependencies(Dependency),
                   OS.Linux                  => GetAvailableLinuxDependencies(Dependency),
                   OS.Osx                    => GetAvailableOsxDependencies(Dependency),
                   _                         => throw new PlatformNotSupportedException($"Cannot install dependencies for unknown OS: {CurrentPlatform.LongDescription}")
               };

        protected InvalidOperationException DependencyUnsetException => throw new InvalidOperationException("Dependency has not been set and operations depending on it may not execute.");

        public abstract ValueTask<bool> IsAlreadyInstalled();
    }
}