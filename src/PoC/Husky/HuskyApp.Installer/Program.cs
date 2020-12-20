using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.TaskConfiguration.Resources;
using Husky.Core.TaskConfiguration.Scripting;
using Husky.Core.TaskConfiguration.Utilities;
using Husky.Core.Workflow;
using Husky.Installer;
using Husky.Tasks.Resources;
using Husky.Tasks.Scripting;
using Husky.Tasks.Utilities;

namespace HuskyApp.Installer
{   
    /*
     * Todo: Step & Global variables
     * Theories:
     *
     * Global: Let's avoid something fancy for now and stick with something simple. "Simple.
     * Pass in raw strings all the way through.
     * Create a ConfigureVariables method in the IHuskyWorkflowBuilder that allows the simple passing through of raw strings into a list or so.
     * Then on the installer's initialization it can do a once-over on the global vars to tokenize & initialize them
     *
     * Step: Step variables can be a combination of both local and global. This will necessitate two services, with the local service (scoped lifetime)
     * depending on the global service (static lifetime). This combination, along with appropriate reuse of a tokenizer, should be able to properly
     * resolve & initialize all variables.
     *
     */

    public static class Program
    {
        private static readonly HuskyStepConfiguration _unixConfiguration = new(SupportedPlatforms.UnixSystems);
        private static readonly HuskyStepConfiguration _windowsConfiguration = new(SupportedPlatforms.Windows);

        public static async Task Main(string[] args)
        {
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
                                             a.Dependencies = new[]
                                             {
                                                     new HuskyDependency("DotNet", "5")
                                             };
                                         })
                                         .AddGlobalVariable("installDir", $"{HuskyVariables.Folders.ProgramFiles}")
                                        .WithDefaultStage(
                                             stage => stage.SetDefaultStepConfiguration(new HuskyStepConfiguration(SupportedPlatforms.All))
                                                           .AddJob(
                                                                "show-splash",
                                                                splash => splash.AddStep<ExecuteInlineScriptOptions>(
                                                                                     "show-unix-splash",
                                                                                     task => task.Script = linuxScript,
                                                                                     _unixConfiguration)
                                                                                .AddStep<ExecuteInlineScriptOptions>(
                                                                                     "show-windows-splash",
                                                                                     task => task.Script = windowsScript,
                                                                                     _windowsConfiguration))
                                                           .AddJob(
                                                                "extract-husky-app",
                                                                extract => extract.AddStep<ExtractBundledResourceOptions>(
                                                                    "extract-files",
                                                                    task =>
                                                                    {
                                                                        task.Clean = true;
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

            var installer = new HuskyInstaller(workflow);

            await installer.Install();
        }
    }
}