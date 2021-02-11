using System;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskConfiguration.Resources;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.TaskConfiguration.Utilities;
using Husky.Core.Workflow;
using Husky.Installer;

namespace HuskyApp.Installer
{   
    public static class Program
    {
        private static readonly HuskyStepConfiguration LunixConfiguration = new(OS.Linux);
        private static readonly HuskyStepConfiguration WindowsConfiguration = new(OS.Windows);

        public static async Task Main(string[] args)
        {
            /*
             * Todo: When we fully flesh out the Source Generator pattern, we should likely remove this kind of thing from the users' visibility.
             *       Husky should elegantly manage its requisite startup _stuff_ (such as arg parsing) in generated classes,
             *       while exposing extensibility points for the user to make their own modifications.
             *
             *       One possible solution is to find a way to give the user access to the ServiceProvider and possible allow some sort of Middleware
             *       that executes per-step, per-job, per-stage, etc..
             */
            var installationSettings = new HuskyInstallerSettings();
            var argParsingResult = await installationSettings.LoadFromStartArgs(args);
            if (argParsingResult != 0)
                return;

            //HelloWorldGenerated.HelloWorld.SayHello();
            const string linuxScript = @"
cls &&
echo Welcome to Husky-App! &&
read -n 1 -r -s -p $'Press any key to continue installation...\n'";

            const string windowsScript = @"
cls &&
echo Welcome to Husky-App! &&
pause";

            var workflow = HuskyWorkflow.Create()
                                        .Configure<AuthorConfiguration>(a =>
                                         {
                                             a.Publisher = "Sven";
                                             a.PublisherUrl = "https://sven.ai";
                                         })
                                        .Configure<ApplicationConfiguration>(a =>
                                         {
                                             a.Name = "HuskyApp";
                                             a.Version = "1.0.0";
                                         })
                                        .Configure<ClientMachineRequirementsConfiguration>(c =>
                                         {
                                             c.SupportedOperatingSystems = new[] { OS.Windows, OS.Linux };
                                             c.FreeSpaceMb = 128;
                                             c.MemoryMb = 1024;
                                         })
                                        .AddDependency(new DotNet(Range: ">=5.0.0", FrameworkType: FrameworkInstallationType.Runtime, Kind: DotNet.RuntimeKind.RuntimeOnly))
                                        .AddGlobalVariable("installDir", $"{HuskyVariables.Folders.ProgramFiles}")
                                        .WithDefaultStage(
                                             stage => stage
                                                           .AddJob(
                                                                "show-splash",
                                                                splash => splash.SetDefaultStepConfiguration(new HuskyStepConfiguration(CurrentPlatform.OS)
                                                                                 {
                                                                                     Tags = new[] { "Install" }
                                                                                 })
                                                                                .AddStep<ExecuteInlineScriptOptions>(
                                                                                     "show-unix-splash",
                                                                                     task => task.Script = linuxScript,
                                                                                     LunixConfiguration)
                                                                                .AddStep<ExecuteInlineScriptOptions>(
                                                                                     "show-windows-splash",
                                                                                     task => task.Script = windowsScript,
                                                                                     WindowsConfiguration))
                                                           .AddJob(
                                                                "extract-husky-app",
                                                                extract => extract.AddStep<ExtractBundledResourceOptions>(
                                                                    "extract-files",
                                                                    task =>
                                                                    {
                                                                        task.CleanDirectories = true;
                                                                        task.CleanFiles = true;
                                                                        task.Resources = "dist/program/**/*";
                                                                        task.TargetDirectory = "$variables.installDir";
                                                                    }))
                                                           .AddJob(
                                                                "create-launch-file",
                                                                launch => launch.AddStep<CreateScriptFileOptions>(
                                                                                     "create-launch-script",
                                                                                     task =>
                                                                                     {
                                                                                         task.Directory = "$variables.installDir";
                                                                                         task.FileName = "launch";
                                                                                         task.Script = "dotnet HuskyApp.dll";
                                                                                     })
                                                                                .AddStep<CreateShortcutOptions>(
                                                                                     "create-shortcut",
                                                                                     task => task.Target = "$variables.create-launch-file.create-launch-script.createdFileName"))
                                         ).Build();

            await new HuskyInstaller(workflow, installationSettings).Execute();
        }
    }
}