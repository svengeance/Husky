using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Workflow;
using Husky.Tasks.Resources;
using Husky.Tasks.Scripting;
using Husky.Tasks.Utilities;

namespace HuskyApp.Installer
{
    public static class Program
    {
        public static void Main(string[] args)
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
                                        .WithDefaultStage(
                                             stage => stage.AddJob(
                                                                "show-splash",
                                                                splash => splash.AddStep<ExecuteInlineScript>(
                                                                                     "show-unix-splash",
                                                                                     step => step.Configure(c => c.Script = linuxScript)
                                                                                                 .SupportedOn(SupportedPlatforms.FreeBSD |
                                                                                                              SupportedPlatforms.Mac |
                                                                                                              SupportedPlatforms.Linux))
                                                                                .AddStep<ExecuteInlineScript>(
                                                                                     "show-windows-splash",
                                                                                     step => step.Configure(c => c.Script = windowsScript)
                                                                                                 .SupportedOn(SupportedPlatforms.Windows)))
                                                           .AddJob(
                                                                "extract-husky-app",
                                                                extract => extract.AddStep<ExtractBundledResource>(
                                                                    "extract-files",
                                                                    step => step.Configure(c =>
                                                                    {
                                                                        c.Clean = true;
                                                                        c.Resources = "dist/program/**/*";
                                                                        c.TargetDirectory = "$variables.installDir";
                                                                    })))
                                                           .AddJob(
                                                                "create-launch-file",
                                                                launch => launch.AddStep<CreateScriptFile>(
                                                                                     "create-launch-script",
                                                                                     step => step.Configure(c =>
                                                                                     {
                                                                                         c.Directory = "$variables.installDir";
                                                                                         c.FileName = "launch";
                                                                                         c.Script = "dotnet HuskyApp.dll";
                                                                                     }))
                                                                                .AddStep<CreateShortcut>(
                                                                                     "create-shortcut",
                                                                                     step => step.Configure(c
                                                                                         => c.Target =
                                                                                             "$variables.create-launch-file.create-launch-script.createdFileName")))
                                         ).Build();
        }
    }
}