using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static Husky.Core.HuskyConstants.Workflows;

namespace Husky.Installer
{
    public class HuskyInstallerSettings
    {
        public IEnumerable<Assembly> ResolveModulesFromAssemblies { get; set; } = Array.Empty<Assembly>();
        public string TagToExecute { get; private set; } = string.Empty;
        // Todo: GH #18 - Make DryRun actually do something
        public bool IsDryRun { get; private set; } = false;
        public LogLevel LogLevel { get; private set; } = LogLevel.Information;

        public Task<int> LoadFromStartArgs(string[] args)
        {
            var dryOption = new Option<bool>(new[] { "--dry-run", "-d" }, () => false, "Executes the Husky Workflow without modifying the current machine.");
            var logLevelOption = new Option<LogLevel>(new[] { "--verbosity", "-v" }, () => LogLevel.Information, "Sets the verbosity of Husky's logging.");

            void ParseResult(string tag, ParseResult parsedResult)
            {
                TagToExecute = tag;
                IsDryRun = parsedResult.ValueForOption(dryOption!);
                LogLevel = parsedResult.ValueForOption(logLevelOption!);
            }
            
            ICommandHandler CreateHandler(string tag) => CommandHandler.Create((ParseResult parseResult) => ParseResult(tag, parseResult));

            var rootCommand = new RootCommand
            {
                new Command(StepTags.Install, $"Executes the Tasks within this Workflow tagged '{StepTags.Install}'")
                {
                    Handler = CreateHandler(StepTags.Install)
                },
                new Command(StepTags.Uninstall, $"Executes the Tasks within this Workflow tagged '{StepTags.Uninstall}'")
                {
                    Handler = CreateHandler(StepTags.Uninstall)
                },
                new Command(StepTags.Modify, $"Executes the Tasks within this Workflow tagged '{StepTags.Modify}'")
                {
                    Handler = CreateHandler(StepTags.Modify)
                },
                new Command(StepTags.Repair, $"Executes the Tasks within this Workflow tagged '{StepTags.Repair}'")
                {
                    Handler = CreateHandler(StepTags.Repair)
                },
                new Command("validate", $"Executes no Tasks within this Workflow, but ensures all options are correctly configured.")
                {
                    Handler = CreateHandler(string.Empty)
                }
            };
            
            rootCommand.AddGlobalOption(dryOption);
            rootCommand.AddGlobalOption(logLevelOption);
            var parser = new CommandLineBuilder(rootCommand)
                        .UseDefaults() // Todo: Logging, log this to the file instead of Console Out
                        .UseMiddleware(c => Console.WriteLine(c.ParseResult.Diagram()))
                        .Build();

            return parser.InvokeAsync(args);
        }
    }
}