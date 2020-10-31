using System.Threading.Tasks;
using Husky.Core.Workflow;

namespace Husky.Installer
{
    public class Installer
    {
        private readonly HuskyWorkflow _workflow;

        public Installer(HuskyWorkflow workflow)
        {
            _workflow = workflow;
        }

        public async Task Install()
        {
            
        }
    }
}