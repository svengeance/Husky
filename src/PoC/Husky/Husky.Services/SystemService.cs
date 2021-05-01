using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Serilog;
using Serilog.Core;

namespace Husky.Services
{
    public interface ISystemService
    {
        ValueTask<SystemInformation> GetSystemInformation();
    }

    public class SystemService: ISystemService
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(SystemService));
        private readonly IShellExecutionService _shellExecutionService;

        public SystemService(IShellExecutionService shellExecutionService)
        {
            _shellExecutionService = shellExecutionService;
        }

        public async ValueTask<SystemInformation> GetSystemInformation()
        {
            _logger.Information("Retrieving information about the current system");
            _logger.Verbose("Retrieving total memory");
            var totalMemory = CurrentPlatform.OS switch
                              {
                                  OS.Windows => GetTotalAvailableMemoryMegabytesWindows(),
                                  OS.Linux   => await GetTotalAvailableMemoryMegabytesLinux(),
                                  _ => throw new PlatformNotSupportedException($"Unable to get system information for Platform: {CurrentPlatform.LongDescription}")
                              };

            _logger.Verbose("Retrieving drive information");
            var driveInformation = DriveInfo.GetDrives()
                                            .Select(s => new SystemDriveInformation(s.RootDirectory)
                                             {
                                                 FreeSpaceMb = s.AvailableFreeSpace / 1024 / 1024,
                                             });

            var systemInformation = new SystemInformation
            {
                TotalMemoryMb = totalMemory,
                DriveInformation = driveInformation.ToArray()
            };

            _logger.Debug("Retrieved system information {@systemInformation}", systemInformation);
            return systemInformation;
        }

        private int GetTotalAvailableMemoryMegabytesWindows()
        {
            _logger.Verbose("P/Invoking GlobalMemoryStatusEx");
            MemoryStatusEx memoryStatus = new();
            memoryStatus.dwMemoryLoad = (uint) Marshal.SizeOf<MemoryStatusEx>();
            if (GlobalMemoryStatusEx(ref memoryStatus))
                return (int) Math.Round((memoryStatus.ullTotalPhys / 1024.0 / 1024.0) + (memoryStatus.ullTotalPageFile / 1024.0 / 1024.0));

            throw new Win32Exception(Marshal.GetLastWin32Error(), "Error retrieving the total available memory for the system");
        }

        private async Task<int> GetTotalAvailableMemoryMegabytesLinux()
        {
            _logger.Verbose("Reading /proc/meminfo");
            var memInfoQuery = await _shellExecutionService.ExecuteShellCommand("head -n 1 /proc/meminfo");
            if (!memInfoQuery.WasSuccessful) // Todo: Generic "command failed to execute" exception w/ attempted command
                throw new ApplicationException($"Unable to inspect system total memory - {CurrentPlatform.LongDescription}");

            var memoryInKilobytesString = memInfoQuery.StdOutput.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
            return int.Parse(memoryInKilobytesString) / 1024;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MemoryStatusEx
        {
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);
    }

    public class SystemInformation
    {
        public int TotalMemoryMb { get; init; }
        public SystemDriveInformation[] DriveInformation { get; init; } = Array.Empty<SystemDriveInformation>();

        internal SystemInformation() { }
    }

    public class SystemDriveInformation
    {
        public DirectoryInfo RootDirectory { get; init; }
        public long FreeSpaceMb { get; init; }

        internal SystemDriveInformation(DirectoryInfo rootDirectory) => RootDirectory = rootDirectory;
    }
}