using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Microsoft.Extensions.Logging;

namespace Husky.Services
{
    public interface IShellExecutionService
    {
        ValueTask<ShellExecutionService.ScriptExecutionResult> ExecuteShellCommand(string command);

        ValueTask<ShellExecutionService.ScriptExecutionResult> ExecuteFile(string filePath, string args);

        string GetShellExecuteAndTerminateArg();

        string GetShellFileName();
    }

    public class ShellExecutionService: IShellExecutionService
    {
        private const string WindowsShellExecuteAndTerminateArg = "/c";
        private const string LinuxShellExecuteAndTerminateArg = "-c";
        private const string OsxShellExecuteAndTerminateArg = "-c";

        private const string WindowsShellFileName = "cmd.exe";
        private const string LinuxShellFileName = "/bin/sh";
        private const string OsxShellFileName = "/bin/sh";
        
        private readonly ILogger _logger;

        public ShellExecutionService(ILogger<ShellExecutionService> logger)
        {
            _logger = logger;
        }

        public async ValueTask<ScriptExecutionResult> ExecuteShellCommand(string command)
        {
            _logger.LogInformation("Executing shell command {command}", command);
            var commandResult = await Cli.Wrap(GetShellFileName())
                                         .WithArguments($"{GetShellExecuteAndTerminateArg()} {command}")
                                         .ExecuteBufferedAsync();

            var scriptExecutionResult = new ScriptExecutionResult(commandResult.ExitCode, commandResult.StandardOutput, commandResult.StandardError);
            scriptExecutionResult.LogResult(_logger);
            
            return scriptExecutionResult;
        }

        public async ValueTask<ScriptExecutionResult> ExecuteFile(string filePath, string args)
        {
            // Todo: Going to need a non buffered version so we're not storing all of stdout in memory
            _logger.LogInformation("Executing file {filePath} with args command {command}", filePath, args);
            var commandResult = await Cli.Wrap(filePath)
                                         .WithArguments(args)
                                         .ExecuteBufferedAsync();

            var scriptExecutionResult = new ScriptExecutionResult(commandResult.ExitCode, commandResult.StandardOutput, commandResult.StandardError);
            scriptExecutionResult.LogResult(_logger);
            return scriptExecutionResult;
        }

        public string GetShellExecuteAndTerminateArg()
            => CurrentPlatform.OS switch
               {
                   OS.Windows => WindowsShellExecuteAndTerminateArg,
                   OS.Osx     => OsxShellExecuteAndTerminateArg,
                   _          => LinuxShellExecuteAndTerminateArg,
               };

        public string GetShellFileName()
            => CurrentPlatform.OS switch
               {
                   OS.Windows => WindowsShellFileName,
                   OS.Osx     => OsxShellFileName,
                   _          => LinuxShellFileName,
               };

        public record ScriptExecutionResult(int ExitCode, string StdOutput, string StdError)
        {
            public bool WasSuccessful => ExitCode == 0;

            public void LogResult(ILogger logger)
            {
                logger.LogInformation("Executed shell command with exit code {exitCode}", ExitCode);
                logger.LogDebug("Shell execution resulted in the following StdOutput:\n{standardOutput}", StdOutput);
                logger.LogDebug("Shell execution resulted in the following StdError:\n{standardError}", StdError);
            }
        }
    }
}