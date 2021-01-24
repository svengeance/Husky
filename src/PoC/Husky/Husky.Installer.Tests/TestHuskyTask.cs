using System.Threading.Tasks;
using Husky.Core.Workflow;
using Husky.Tasks;

namespace Husky.Installer.Tests
{
    public class TestHuskyTask: HuskyTask<TestHuskyTaskOptions>
    {
        public bool HasRan = false;
        public bool HasRolledBack = false;

        protected override ValueTask ExecuteTask()
        {
            HasRan = true;
            return ValueTask.CompletedTask;
        }

        protected override ValueTask RollbackTask()
        {
            HasRolledBack = true;
            return ValueTask.CompletedTask;
        }
    }
}