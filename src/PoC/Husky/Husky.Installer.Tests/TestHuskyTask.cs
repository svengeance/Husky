using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;

namespace Husky.Installer.Tests
{
    public class TestHuskyTask: HuskyTask<TestHuskyTaskOptions>
    {
        public bool HasRan = false;
        public bool HasRolledBack = false;

        protected override Task ExecuteTask()
        {
            HasRan = true;
            return Task.CompletedTask;
        }

        protected override Task RollbackTask()
        {
            HasRolledBack = true;
            return Task.CompletedTask;
        }
    }
}