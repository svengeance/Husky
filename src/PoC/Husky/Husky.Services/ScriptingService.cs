using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Husky.Services
{
    public interface IScriptingService
    {
        ProcessStartInfo GetShellProcessStartInfo();
        Task<int> ExecuteCommand(string command, bool showWindow = false);
    }
    
    public class ScriptingService: IScriptingService
    {
        public async Task<int> ExecuteCommand(string command, bool showWindow = false)
        {
            // Todo: Maybe pipe stdout/stderror and return a comprehensive ProcessResult class
            var procInfo = GetShellProcessStartInfo();
            procInfo.ArgumentList.Add(command);
            procInfo.CreateNoWindow = !showWindow;
            procInfo.WindowStyle = showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            
            var proc = new Process { StartInfo = procInfo };
            proc.Start();
            await proc.WaitForExitAsync();

            return proc.ExitCode;
        }
        
        public ProcessStartInfo GetShellProcessStartInfo()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? WindowsCmdStartInfo
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? MacShellStartInfo
                    : LinuxShellStartInfo;

        protected virtual ProcessStartInfo WindowsCmdStartInfo => new("cmd.exe") { ArgumentList = { "/c" }};
        protected virtual ProcessStartInfo LinuxShellStartInfo => new("/bin/bash") { ArgumentList = { "-c" }};
        protected virtual ProcessStartInfo MacShellStartInfo => new("cmd.exe") { ArgumentList = { "-c" } };
    }
}