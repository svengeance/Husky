using System;
using Husky.Core.Workflow;
using StrongInject;

namespace Husky.Tasks.Infrastructure
{
    public interface IHuskyTaskResolver
    {
        Owned<HuskyTask<HuskyTaskConfiguration>> ResolveTaskForConfiguration(HuskyContext context, Type configurationType);
    }

    public class HuskyTaskResolver: IHuskyTaskResolver
    {
        public Owned<HuskyTask<HuskyTaskConfiguration>> ResolveTaskForConfiguration(HuskyContext context, Type configurationType)
        {
            using var container = new HuskyTasksContainer(context);
            return container.ResolveTaskForConfiguration(configurationType);
        }
    }
}