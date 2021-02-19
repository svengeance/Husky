using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using static Husky.Core.HuskyConstants.StepTags;

namespace Husky.Installer
{
    public class HuskyInstallerSettings
    {
        public IEnumerable<Assembly> ResolveModulesFromAssemblies { get; set; } = Array.Empty<Assembly>();
        public LoggerConfiguration LoggerConfiguration { get; set; }
        public string TagToExecute { get; private set; } = string.Empty;
        // Todo: GH #18 - Make DryRun actually do something
        public bool IsDryRun { get; private set; }
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
                new Command(Install, $"Executes the Tasks within this Workflow tagged '{Install}'")
                {
                    Handler = CreateHandler(Install)
                },
                new Command(Uninstall, $"Executes the Tasks within this Workflow tagged '{Uninstall}'")
                {
                    Handler = CreateHandler(Uninstall)
                },
                new Command(Modify, $"Executes the Tasks within this Workflow tagged '{Modify}'")
                {
                    Handler = CreateHandler(Modify)
                },
                new Command(Repair, $"Executes the Tasks within this Workflow tagged '{Repair}'")
                {
                    Handler = CreateHandler(Repair)
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