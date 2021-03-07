using System.Threading.Tasks;
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
    }
}