using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Husky.Core.HuskyConfiguration;
using Husky.Core.TaskConfiguration.Utilities;
using Husky.Services;

namespace Husky.Tasks.Utilities
{
    // Todo: Add proper error handling
    public class CreateShortcut : HuskyTask<CreateShortcutOptions>
    {
        private string _windowsShortcutFileExtension = ".lnk";
        private string _unixShortcutFileExtension = ".desktop";
        
        private readonly IScriptingService _scriptingService;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public CreateShortcut(IScriptingService scriptingService, ApplicationConfiguration applicationConfiguration)
        {
            _scriptingService = scriptingService;
            _applicationConfiguration = applicationConfiguration;
        }
        
        protected override async Task ExecuteTask()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsShortcut();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                await CreateLinuxShortcut();
            else
                throw new NotImplementedException($"Unable to create a Shortcut for the OSPlatform: {RuntimeInformation.OSDescription}");
        }

        private void CreateWindowsShortcut()
        {   // Lord forgive me for my sins.
            // ReSharper disable once SuspiciousTypeConversion.Global (cast is definitely sus)
            IShellLink link = (IShellLink) new ShellLink();

            link.SetDescription(Configuration.Comment);
            link.SetPath(Configuration.Target);
            link.SetArguments(Configuration.Arguments);

            if (!string.IsNullOrWhiteSpace(Configuration.ShortcutImageFilePath))
                link.SetIconLocation(Configuration.ShortcutImageFilePath, 0);

            // ReSharper disable once SuspiciousTypeConversion.Global
            IPersistFile file = (IPersistFile) link;
            var pathToSave = Path.Combine(Configuration.ShortcutLocation, Configuration.ShortcutName + _windowsShortcutFileExtension);
            file.Save(pathToSave, false);
        }

        private async Task CreateLinuxShortcut()
        {
            (string key, string value)[] desktopFileSettings =
            {
                ("Version", _applicationConfiguration.Version),
                ("Name", Configuration.ShortcutName),
                ("Comment", Configuration.Comment),
                ("Exec", $"{Configuration.Target} {Configuration.Arguments}"),
                ("Icon", Configuration.ShortcutImageFilePath ?? string.Empty),
                ("Terminal", Configuration.UnixStartUseTerminal.ToString()),
                ("Type", Configuration.UnixShortcutType),
                ("Categories", Configuration.UnixShortcutCategories),
            };

            var desktopStringBuilder = new StringBuilder();
            desktopStringBuilder.AppendLine("[Desktop Entry]");

            foreach (var (key, value) in desktopFileSettings)
                desktopStringBuilder.Append(key).Append('=').AppendLine(value);

            var path = Path.Combine(Configuration.ShortcutLocation, Configuration.ShortcutName + _unixShortcutFileExtension);
            await File.WriteAllTextAsync(path, desktopStringBuilder.ToString());
        }

        protected override Task RollbackTask()
        {
            /*
             * Todo: We should probably have a comprehensive HuskyNotSupportException that encapsulates the current operation,
             *       the current operating system etc..
             */
            var fileExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? _windowsShortcutFileExtension
                : _unixShortcutFileExtension
                ?? throw new NotImplementedException($"Unable to rollback Shortcut for the OSPlatform: {RuntimeInformation.OSDescription}");

            var fileThatShouldExist = new FileInfo(Path.Combine(Configuration.ShortcutLocation, Configuration.ShortcutName + fileExtension));
            if (fileThatShouldExist.Exists)
                fileThatShouldExist.Delete();

            return Task.CompletedTask;
        }

        /*
         * Stolen shamelessly from https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
         * Todo: We need to test functionality in both 64/32bit contexts
         */
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}