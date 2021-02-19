using System;
using System.IO;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Microsoft.Extensions.Logging;

namespace Husky.Services
{
    public interface IFileSystemService
    {
        ValueTask<FileInfo> WriteToFile(Stream stream, string? filePath = null, long? totalLength = null, IProgress<FileSystemService.FileWriteProgress>? bytesWrittenProgress = null);

        DirectoryInfo CreateTempDirectory();
        
        ValueTask<string> CreateScriptFile(string destinationDirectory, string fileName, string script);
        
        string GetScriptFileExtension();
    }

    public class FileSystemService: IFileSystemService
    {
        private readonly ILogger _logger;
        protected virtual string WindowsScriptFileExtension => ".cmd";
        protected virtual string LinuxScriptFileExtension => ".sh";
        protected virtual string OsxScriptFileExtension => ".sh";

        public FileSystemService(ILogger<FileSystemService> logger)
        {
            _logger = logger;
        }

        public DirectoryInfo CreateTempDirectory()
        {
            _logger.LogDebug("Creating a temp directory");
            var directoryName = Path.GetRandomFileName();
            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), directoryName));
            directoryInfo.Create();
            _logger.LogInformation("Temp directory created at {tempDirectoryPath}", directoryInfo.FullName);

            return directoryInfo;
        }

        public async ValueTask<FileInfo> WriteToFile(Stream stream, string? filePath = null, long? totalLength = null,
            IProgress<FileWriteProgress>? bytesWrittenProgress = null)
        {
            totalLength ??= 0L;
            filePath ??= Path.GetTempFileName();
            _logger.LogInformation("Writing {totalLength} bytes to {filePath}", totalLength, filePath);

            var totalBytesRead = 0L;
            int bytesRead;
            var buffer = new byte[4 * 1024];

            await using var fs = File.OpenWrite(filePath);

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;
                bytesWrittenProgress?.Report(new FileWriteProgress((long) totalLength, totalBytesRead));
            }

            // Todo: Verify file was written && log otherwise
            return new FileInfo(filePath);
        }

        /*
         * Todo: Going to need to acquire some wrapper or another to set 755 perms on Linux systems
         */
        public async ValueTask<string> CreateScriptFile(string destinationDirectory, string fileName, string script)
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

        public string GetScriptFileExtension()
            => CurrentPlatform.OS switch
               {
                   OS.Windows => WindowsScriptFileExtension,
                   OS.Osx     => OsxScriptFileExtension,
                   _          => LinuxScriptFileExtension,
               };


        public readonly struct FileWriteProgress
        {
            public readonly long BytesLength;
            public readonly long BytesWritten;

            public string ProgressPercent => $"{BytesWritten / BytesLength:P0}";

            public FileWriteProgress(long bytesLength, long bytesWritten)
            {
                BytesLength = bytesLength;
                BytesWritten = bytesWritten;
            }
        }
    }
}