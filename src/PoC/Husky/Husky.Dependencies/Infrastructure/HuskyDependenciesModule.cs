using Husky.Dependencies.Services;
using StrongInject;

namespace Husky.Dependencies.Infrastructure
{
    [Register(typeof(DependencyHandlerResolver), typeof(IDependencyHandlerResolver))]
    [Register(typeof(DependencyAcquisitionService), typeof(IDependencyAcquisitionService))]
    public class HuskyDependenciesModule { }
}