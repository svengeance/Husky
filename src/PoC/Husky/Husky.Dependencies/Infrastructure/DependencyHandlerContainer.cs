using System;
using System.Runtime.CompilerServices;
using Husky.Core.Dependencies;
using Husky.Dependencies.DependencyHandlers;
using Husky.Services.Infrastructure;
using StrongInject;

namespace Husky.Dependencies.Infrastructure
{
    [Register(typeof(DotNetDependencyHandler))]
    [RegisterModule(typeof(HuskyServicesModule))]
    public partial class DependencyHandlerContainer: IContainer<DotNetDependencyHandler>
    {
        public Owned<IDependencyHandler> ResolveDependencyHandlerForDependency(HuskyDependency dependency)
        {
            object result;

            if (dependency is DotNet dotnet)
            {
                var dotnetDependencyHandler = this.Resolve<DotNetDependencyHandler>();
                dotnetDependencyHandler.Value.LoadDependency(dotnet);
                result = dotnetDependencyHandler;
            }
            else
            {
                throw new ArgumentException($"Unable to resolve a Handler for Dependency {dependency}", nameof(dependency));
            }

            return Unsafe.As<Owned<IDependencyHandler>>(result);
        }
    }
}