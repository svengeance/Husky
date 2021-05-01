using Husky.Core.HuskyConfiguration;
using StrongInject;

namespace Husky.Core.Infrastructure
{
    [Register(typeof(ApplicationConfiguration))]
    [Register(typeof(AuthorConfiguration))]
    [Register(typeof(ClientMachineRequirementsConfiguration))]
    [Register(typeof(InstallationConfiguration))]
    public class HuskyCoreModule { }
}