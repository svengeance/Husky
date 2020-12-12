using System.Threading.Tasks;
using Husky.Core.Workflow;

namespace Husky.Installer.Tests
{
    public class TestHuskyTask: HuskyTask<TestHuskyTaskOptions>
    {
        public bool HasRan = false;
        public bool HasRollbacked = false;

        protected override Task ExecuteTask()
        {
            HasRan = true;
            return Task.CompletedTask;
        }

        protected override Task RollbackTask()
        {
            HasRollbacked = true;
            return Task.CompletedTask;
        }
    }
}