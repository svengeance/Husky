using StrongInject;

namespace Husky.Services.Infrastructure
{
    [Register(typeof(EmbeddedResourceService), typeof(IEmbeddedResourceService))]
    [Register(typeof(FileSystemService), typeof(IFileSystemService))]
    [Register(typeof(HttpService), typeof(IHttpService))]
    [Register(typeof(RegistryService), typeof(IRegistryService))]
    [Register(typeof(ShellExecutionService), typeof(IShellExecutionService))]
    [Register(typeof(SystemService), typeof(ISystemService))]
    [Register(typeof(VariableResolverService), typeof(IVariableResolverService))]
    public class HuskyServicesModule { }
}