using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.Platform;

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

        public async ValueTask<ScriptExecutionResult> ExecuteShellCommand(string command)
        {
            var commandResult = await Cli.Wrap(GetShellFileName())
                                         .WithArguments($"{GetShellExecuteAndTerminateArg()} {command}")
                                         .ExecuteBufferedAsync();

            return new ScriptExecutionResult(commandResult.ExitCode, commandResult.StandardOutput, commandResult.StandardError);
        }

        public async ValueTask<ScriptExecutionResult> ExecuteFile(string filePath, string args)
        {
            // Todo: Going to need a non buffered version so we're not storing all of stdout in memory
            var commandResult = await Cli.Wrap(filePath)
                                         .WithArguments(args)
                                         .ExecuteBufferedAsync();

            return new ScriptExecutionResult(commandResult.ExitCode, commandResult.StandardOutput, commandResult.StandardError);
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
        }
    }
}