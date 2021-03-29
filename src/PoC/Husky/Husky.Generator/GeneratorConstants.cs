using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Husky.Generator
{
    public static class GeneratorConstants
    {
        public const string WorkflowConfigurationFileName = "Husky";
        public const string GeneratedWorkflowFileName = "HuskyWorkflow.cs";

        public static class Workflow
        {
            public const string DefaultStageName = "DefaultStage";
            public const string DefaultJobName = "DefaultJob";

            public static class YamlBlocks
            {
                public const int RootColumnIndex = 3;

                public const string Dependencies = "dependencies";
                public const string Jobs = "jobs";
                public const string Stages = "stages";
                public const string Steps = "steps";
                public const string Variables = "variables";

                public static class JobProperties
                {
                    public const string Tags = "tags";
                }

                public static class StepProperties
                {
                    public const string Platforms = "platforms";
                    public const string Task = "task";
                    public const string With = "with";
                }
            }
        }
    }
}