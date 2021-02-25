using System;
using Husky.Core.Dependencies;
using Husky.Core.HuskyConfiguration;
using Husky.Dependencies.DependencyHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Husky.Dependencies
{
    public interface IDependencyHandlerResolver
    {
        IDependencyHandler Resolve(HuskyDependency dependency);
    }

    public class DependencyHandlerResolver: IDependencyHandlerResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyHandlerResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDependencyHandler Resolve(HuskyDependency dependency)
            => dependency switch
               {
                   DotNet dotnet => ActivatorUtilities.CreateInstance<DotNetDependencyHandler>(_serviceProvider, dotnet),
                   _             => throw new ArgumentOutOfRangeException(nameof(dependency), $"No handler for dependency {dependency}")
               };
    }
}