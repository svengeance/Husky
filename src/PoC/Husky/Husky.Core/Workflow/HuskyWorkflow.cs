using System.Collections.Generic;
using Husky.Core.Builder;
using Husky.Core.HuskyConfiguration;

namespace Husky.Core.Workflow
{
    public sealed class HuskyWorkflow
    {
        public HuskyConfiguration Configuration { get; }
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();
        public List<HuskyStage> Stages { get; } = new List<HuskyStage>();

        internal HuskyWorkflow(HuskyConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IHuskyWorkflowBuilder Create() => new WorkflowBuilder(new HuskyWorkflow(HuskyConfiguration.Create()));
    }
}