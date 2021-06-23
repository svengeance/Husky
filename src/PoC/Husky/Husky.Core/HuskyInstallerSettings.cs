using System;
using Husky.Core.CLI;
using Serilog.Events;

namespace Husky.Core
{
    public class HuskyInstallerSettings
    {
        public string TagToExecute { get; private set; } = string.Empty;

        // Todo: GH #18 - Make DryRun actually do something
        public bool IsDryRun { get; private set; }
        public LogEventLevel LogLevel { get; private set; } = LogEventLevel.Information;

        public int LoadFromStartArgs(string[] args)
        {
            var parseResult = HuskyCommandLineParser.Parse(args);

            if (parseResult is null)
                return 1;

            TagToExecute = parseResult.Command;
            IsDryRun = parseResult.TryGetOption("dry-run", out var dryRun) && dryRun.GetAsBool();
            if (parseResult.TryGetOption("verbosity", out var logLevel))
                LogLevel = Enum.Parse<LogEventLevel>(logLevel.GetAsString());

            return 0;
        }
    }
}