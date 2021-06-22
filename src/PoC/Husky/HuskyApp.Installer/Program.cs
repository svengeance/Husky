using System;
 using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Attributes;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Husky.Core.Workflow;
using Husky.Installer;
using Husky.Installer.WorkflowExecution;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace HuskyApp.Installer
{   
    public static class Program
    {
        public static class LogConfiguration
        {
            public const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Stage}{Job}{Task}({SourceContext}) {Message}{NewLine}{Exception}";
            public const string FileTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Stage}{Job}{Task}({SourceContext}) {Message}{NewLine}{Exception}";
            public static string FileDirectory = Path.GetTempPath();
            public const string FileName = "HuskyApp_InstallLog";

            public const string SeqHttpUrl = "http://husky-test-seq:5341";

            public static string JsonLogPath => Path.Combine(FileDirectory, FileName + ".json");
            public static string FlatLogPath => Path.Combine(FileDirectory, FileName + ".txt");
        }

        [HuskyEntryPoint]
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
            logger.Debug("Husky started with args {args}", args);
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
            var installationSettings = new HuskyInstallerSettings();
            var argParsingResult = installationSettings.LoadFromStartArgs(args);

            logger.Information("Parsed installations settings with result {parseResult}", argParsingResult);
            if (argParsingResult != 0)
                return;

            logger.Debug("Finished parsing HuskyInstallerSettings: {@HuskyInstallerSettings}", installationSettings);

            var workflow = Husky.Generated.Workflow.Create();

            var (numStages, numJobs, numTasks) = CountWorkflowItems(workflow);
            logger.Information("Parsed HuskyWorkflow, found {numberOfStages} stages, {numberOfJobs} jobs, and {numberOfTasks} tasks", numStages, numJobs, numTasks);
            logger.Verbose("Workflow\n{@workflow}", workflow);

            try
            {
                WorkflowExecutionBase workflowExecutionOperation =
                    installationSettings.TagToExecute switch
                    {
                        HuskyConstants.StepTags.Install => new HuskyInstaller(workflow, installationSettings),
                        HuskyConstants.StepTags.Uninstall => new HuskyUninstaller(workflow, installationSettings),
                        _ => throw new InvalidOperationException($"Unable to execute Husky with step tag {installationSettings.TagToExecute}")
                    };

                await workflowExecutionOperation.Execute();
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
        }

        private static (int numStages, int numJobs, int numTasks) CountWorkflowItems(HuskyWorkflow workflow)
            => (workflow.Stages.Count, workflow.Stages.Sum(s => s.Jobs.Count), workflow.Stages.Sum(s => s.Jobs.Sum(s2 => s2.Steps.Count)));
    }
}