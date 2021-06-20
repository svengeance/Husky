using Husky.Core.Infrastructure;
using Husky.Services.Infrastructure;
using StrongInject;

namespace Husky.Installer.Tests
{
    [RegisterModule(typeof(HuskyServicesModule))]
    [RegisterModule(typeof(HuskyCoreModule))]
    [Register(typeof(TestHuskyTask))]
    public partial class TestTaskContainer: IContainer<TestHuskyTask>
    {
        
    }
}