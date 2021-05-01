using StrongInject;

namespace Husky.Tasks.Infrastructure
{
    [Register(typeof(HuskyTaskResolver), typeof(IHuskyTaskResolver))]
    public class HuskyTasksModule { }
}