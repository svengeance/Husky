using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Resources;
using Husky.Core.TaskOptions.Scripting;
using Husky.Core.TaskOptions.Utilities;
using Husky.Core.Workflow;
using Husky.Installer;
using Husky.Installer.Lifecycle;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace HuskyApp.Installer
{   
    public static class Program
    {
        private static readonly HuskyStepConfiguration LunixConfiguration = new(OS.Linux);
        private static readonly HuskyStepConfiguration WindowsConfiguration = new(OS.Windows);

        public static class LogConfiguration
        {
            public const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Stage}{Job}{Task}({SourceContext}) {Message}{NewLine}{Exception}";
            public const string FileTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Stage}{Job}{Task}({SourceContext}) {Message}{NewLine}{Exception}";
            public static string FileDirectory = Path.GetTempPath();
            public const string FileName = "HuskyApp_InstallLog";

            public const string SeqHttpUrl = "http://seq:5341";

            public static string JsonLogPath => Path.Combine(FileDirectory, FileName + ".json");
            public static string FlatLogPath => Path.Combine(FileDirectory, FileName + ".txt");
        }

        public static async Task Main(string[] args)
        {
            // Todo: Implement LoggingLevelSwitch (https://stackoverflow.com/questions/25477415/how-can-i-reconfigure-serilog-without-restarting-the-application)
            var loggerConfiguration = new LoggerConfiguration()
                                     .MinimumLevel.Verbose()
                                     .Destructure.AsScalar<DirectoryInfo>()
                                     .WriteTo.Seq(LogConfiguration.SeqHttpUrl) // Todo: Only Seq-by-default if debug, and configure URL from env vars
                                     .WriteTo.Console(LogEventLevel.Verbose, LogConfiguration.ConsoleTemplate, theme: SystemConsoleTheme.Colored)
                                     .WriteTo.Async(a => a.File(new JsonFormatter(), path: LogConfiguration.JsonLogPath, restrictedToMinimumLevel: LogEventLevel.Verbose))
                                     .WriteTo.Async(a => a.File(path: LogConfiguration.FlatLogPath, outputTemplate: LogConfiguration.FileTemplate, restrictedToMinimumLevel: LogEventLevel.Verbose));
            
            using var rootLogger = loggerConfiguration.CreateLogger();
            Log.Logger = rootLogger;
            var logger = rootLogger.ForContext(typeof(Program));
            Serilog.Debugging.SelfLog.Enable(logger.Error);
            logger.Information("Husky & Logger Successfully Initialized");
            logger.Debug("Logging to {loggerFilePath}", LogConfiguration.JsonLogPath);
            logger.Debug("Husy started with args {args}", args);
            logger.Information("Waiting for Seq connection at {seqUrl}", LogConfiguration.SeqHttpUrl);

            var client = new HttpClient();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(1));
            var ct = cts.Token;
            int connectionAttempts = 0;
            while (!ct.IsCancellationRequested)
            {
                logger.Debug("Awaiting connection to Seq, attempt #{connectionAttemptNumber}", ++connectionAttempts);

                var request = await client.GetAsync(LogConfiguration.SeqHttpUrl + "/api", HttpCompletionOption.ResponseHeadersRead, ct);
                if (request.IsSuccessStatusCode)
                {
                    logger.Information("Successfully connected to seq server");
                    break;
                }

                await Task.Delay(1000, ct);
            }

            /*
             * Todo: When we fully flesh out the Source Generator pattern, we should likely remove this kind of thing from the users' visibility.
             *       Husky should elegantly manage its requisite startup _stuff_ (such as arg parsing) in generated classes,
             *       while exposing extensibility points for the user to make their own modifications.
             *
             *       One possible solution is to find a way to give the user access to the ServiceProvider and possible allow some sort of Middleware
             *       that executes per-step, per-job, per-stage, etc..
             */
            var installationSettings = new HuskyInstallerSettings { LoggerConfiguration = loggerConfiguration };
            var argParsingResult = await installationSettings.LoadFromStartArgs(args);

            logger.Information("Parsed installations settings with result {parseResult}", argParsingResult);
            if (argParsingResult != 0)
                return;

            logger.Debug("Finished parsing HuskyInstallerSettings: {@HuskyInstallerSettings}", installationSettings);

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
                                                                        task.Resources = "**/*";
                                                                        task.TargetDirectory = "{Folders.ProgramFiles}/HuskyApp";
                                                                    }))
                                                           .AddJob(
                                                                "create-launch-file",
                                                                launch => launch.AddStep<CreateScriptFileOptions>(
                                                                                     "create-launch-script",
                                                                                     task =>
                                                                                     {
                                                                                         task.Directory = "{Folders.ProgramFiles}/HuskyApp";
                                                                                         task.FileName = "launch";
                                                                                         task.Script = "dotnet \"{Folders.ProgramFiles}/HuskyApp/HuskyApp.dll\"";
                                                                                     })
                                                                                .AddStep<CreateShortcutOptions>(
                                                                                     "create-shortcut",
                                                                                     task =>
                                                                                     {
                                                                                         task.ShortcutName = "Husky App";
                                                                                         task.ShortcutLocation = "{Folders.Desktop}";
                                                                                         task.Target = "{create-launch-file.create-launch-script.createdFileName}";
                                                                                     }))
                                         ).Build();

            var (numStages, numJobs, numTasks) = CountWorkflowItems(workflow);
            logger.Information("Parsed HuskyWorkflow, found {numberOfStages} stages, {numberOfJobs} jobs, and {numberOfTasks} tasks", numStages, numJobs, numTasks);
            logger.Verbose("Workflow\n{@workflow}", workflow);

            try
            {
                LifecycleBase lifecycleOperation =
                    installationSettings.TagToExecute switch
                    {
                        HuskyConstants.StepTags.Install => new HuskyInstaller(workflow, installationSettings),
                        HuskyConstants.StepTags.Uninstall => new HuskyUninstaller(workflow, installationSettings),
                        _ => throw new InvalidOperationException($"Unable to execute Husky with step tag {installationSettings.TagToExecute}")
                    };

                await lifecycleOperation.Execute();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "HuskyInstaller encountered an exception and was unable to recover -- exiting");
                logger.Fatal("Current platform:\n{currentPlatform}", CurrentPlatform.LongDescription);
                logger.Fatal("Husky Workflow:\n{@workFlow}", workflow);
            }
            finally
            {
                logger.Information("Shutting down logger");
                Log.CloseAndFlush();
            }

            // Todo: Wrap main in other method so app-wide concerns like this aren't messy
            Console.ReadLine();
        }

        private static (int numStages, int numJobs, int numTasks) CountWorkflowItems(HuskyWorkflow workflow)
            => (workflow.Stages.Count, workflow.Stages.Sum(s => s.Jobs.Count), workflow.Stages.Sum(s => s.Jobs.Sum(s2 => s2.Steps.Count)));
    }
}