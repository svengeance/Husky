using Husky.Core.Dependencies;
using Husky.Core.HuskyConfiguration;
using StrongInject;

namespace Husky.Dependencies.Infrastructure
{
    public interface IDependencyHandlerResolver
    {
        Owned<IDependencyHandler> Resolve(HuskyDependency dependency);
    }

    public class DependencyHandlerResolver: IDependencyHandlerResolver
    {
        public Owned<IDependencyHandler> Resolve(HuskyDependency dependency)
            => new DependencyHandlerContainer().ResolveDependencyHandlerForDependency(dependency);
    }
}