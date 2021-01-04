using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Husky.Services
{
    public interface IScriptingService
    {
        Task<int> ExecuteCommand(string command, bool showWindow = false);
        Task<string> CreateScriptFile(string destinationDirectory, string fileName, string script);
        string GetScriptFileExtension();
        ProcessStartInfo GetShellProcessStartInfo();
    }
    
    public class ScriptingService: IScriptingService
    {
        public async Task<int> ExecuteCommand(string command, bool showWindow = false)
        {
            /*
             * Todo: Maybe pipe stdout/stderror and return a comprehensive ProcessResult class
             *       The above thought bubble is actually nicely encapsulated by the following lib. Please make note.
             *       https://github.com/Tyrrrz/CliWrap
             */
            var procInfo = GetShellProcessStartInfo();
            procInfo.ArgumentList.Add(command);
            procInfo.CreateNoWindow = !showWindow;
            procInfo.WindowStyle = showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;

            var proc = new Process { StartInfo = procInfo };
            proc.Start();
            await proc.WaitForExitAsync();

            return proc.ExitCode;
        }

        /*
         * Todo: Going to need to acquire some wrapper or another to set 755 perms on Linux systems
         */
        public async Task<string> CreateScriptFile(string destinationDirectory, string fileName, string script)
        {
            var destFileInfo = new FileInfo(Path.Combine(destinationDirectory, fileName + GetScriptFileExtension()));
            var destDirInfo = new DirectoryInfo(destinationDirectory);
            
            if (destDirInfo.Parent is not null && !destDirInfo.Exists) // If we're not a root directory..
                destDirInfo.Create();

            await using var fileWriter = new StreamWriter(destFileInfo.Create());
            await fileWriter.WriteAsync(script);

            destFileInfo.Refresh();
            return destFileInfo.FullName;
        }

        /*
         * Todo: Going to need a healthier way of selecting a value based on the current OS
         *       Something like RunTimeValue<T> that has an underlying list of KVPairs based on (OSPlatform, T)
         */
        public string GetScriptFileExtension()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? WindowsScriptFileExtension
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? MacScriptFileExtension
                    : LinuxScriptFileExtension;

        public ProcessStartInfo GetShellProcessStartInfo()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? WindowsCmdStartInfo
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? MacShellStartInfo
                    : LinuxShellStartInfo;

        protected virtual string WindowsScriptFileExtension => ".cmd";
        protected virtual string LinuxScriptFileExtension => ".sh";
        protected virtual string MacScriptFileExtension => ".sh";
        
        protected virtual ProcessStartInfo WindowsCmdStartInfo => new("cmd.exe") { ArgumentList = { "/c" }};
        protected virtual ProcessStartInfo LinuxShellStartInfo => new("/bin/bash") { ArgumentList = { "-c" }};
        protected virtual ProcessStartInfo MacShellStartInfo => new("/bin/bash") { ArgumentList = { "-c" } };
    }
}