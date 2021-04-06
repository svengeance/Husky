using System;
using Husky.Generator.WorkflowParser.YAML;
using NUnit.Framework;

namespace Husky.Generator.Tests.FullWorkflowTests.HuskyApp
{
    internal class HuskyAppTests: BaseFullWorkflowTest<YamlWorkflowParser>
    {
        protected override string FileName => "FullWorkflowTests/HuskyApp/HuskyApp.yml";

        private const string ExpectedWorkflow = @"namespace Husky.Generated
{
    public static class Workflow
    {
        public static global::Husky.Core.Workflow.HuskyWorkflow Create()
        {
            return global::Husky.Core.Workflow.HuskyWorkflow.Create()
            .Configure<global::Husky.Core.HuskyConfiguration.AuthorConfiguration>(author =>
            {
                author.Publisher = @""Svengeance"";
                author.PublisherUrl = @""https://sven.ai"";
            })
            .Configure<global::Husky.Core.HuskyConfiguration.ApplicationConfiguration>(application =>
            {
                application.Name = @""Husky App"";
                application.Version = @""1.0.0"";
                application.InstallDirectory = @""{Folders.ProgramFiles}/HuskyApp"";
            })
            .Configure<global::Husky.Core.HuskyConfiguration.ClientMachineRequirementsConfiguration>(clientMachineRequirements =>
            {
                clientMachineRequirements.SupportedOperatingSystems = new[] { global::Husky.Core.Enums.OS.Windows, global::Husky.Core.Enums.OS.Linux };
                clientMachineRequirements.FreeSpaceMb = 128;
                clientMachineRequirements.MemoryMb = 1024;
            })
            .AddDependency(new global::Husky.Core.Dependencies.DotNet(Range: @"">=5.0.0"", FrameworkInstallationKind: Husky.Core.Dependencies.DotNet.FrameworkInstallation.Runtime, RuntimeInstallationKind: Husky.Core.Dependencies.DotNet.RuntimeInstallation.RuntimeOnly))
            .WithDefaultStage(
                stage => stage
                .AddJob(
                    ""show-splash"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""show-unix-splash"",
                        task =>
                        {
                            task.Script = @""cls &&
echo Welcome to Husky-App! &&
read -n 1 -r -s -p $'Press any key to continue installation...\n'
"";
                        }
                        ,new(global::Husky.Core.Enums.OS.Linux))
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.ExecuteInlineScriptOptions>(
                        ""show-windows-splash"",
                        task =>
                        {
                            task.Script = @""cls &&
echo Welcome to Husky-App! &&
pause
"";
                        }
                        ,new(global::Husky.Core.Enums.OS.Windows))
                    )
                .AddJob(
                    ""extract-husky-app"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Resources.ExtractBundledResourceOptions>(
                        ""extract-files"",
                        task =>
                        {
                            task.Resources = @""**/*"";
                            task.TargetDirectory = @""{Folders.ProgramFiles}/HuskyApp"";
                        })
                    )
                .AddJob(
                    ""create-launch-file"",
                    job => job
                    .AddStep<global::Husky.Core.TaskOptions.Scripting.CreateScriptFileOptions>(
                        ""create-launch-script"",
                        task =>
                        {
                            task.Directory = @""{Folders.ProgramFiles}/HuskyApp"";
                            task.FileName = @""launch"";
                            task.Script = @""dotnet """"{Folders.ProgramFiles}/HuskyApp/HuskyApp.dll"""""";
                        })
                    .AddStep<global::Husky.Core.TaskOptions.Utilities.CreateShortcutOptions>(
                        ""create-shortcut"",
                        task =>
                        {
                            task.ShortcutLocation = @""{Folders.Desktop}"";
                            task.ShortcutName = @""HuskyApp"";
                            task.Target = @""{create-launch-file.create-launch-script.createdFileName}"";
                        })
                    )
                )
            .Build();
        }
    }
}
";

        [Test]
        [Category("IntegrationTest")]
        public void Husky_app_is_parsed_and_written()
        {
            Console.WriteLine(WorkflowResult);
            Assert.AreEqual(ExpectedWorkflow, WorkflowResult);
        }
    }
}