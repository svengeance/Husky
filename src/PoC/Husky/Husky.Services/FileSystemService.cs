﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.Platform;
using Serilog;
using Serilog.Core;

namespace Husky.Services
{
    public interface IFileSystemService
    {
        DirectoryInfo CreateTempDirectory();
        ValueTask<string> CreateScriptFile(string destinationDirectory, string fileName, string script);
        ValueTask<DirectoryInfo> CreateDirectory(string directoryPath);
        ValueTask DeleteFile(string filePath);
        ValueTask DeleteDirectoryRecursive(string directoryPath);
        ValueTask DeleteDirectory(string directoryPath, bool skipIfNotEmpty = false);
        string GetScriptFileExtension();
        ValueTask<FileInfo> WriteToFile(Stream stream, string? filePath = null, long? totalLength = null, IProgress<FileWriteProgress>? bytesWrittenProgress = null);

        readonly struct FileWriteProgress
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

    public class FileSystemService: IFileSystemService
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(FileSystemService));
        protected virtual string WindowsScriptFileExtension => ".cmd";
        protected virtual string LinuxScriptFileExtension => ".sh";
        protected virtual string OsxScriptFileExtension => ".sh";

        public DirectoryInfo CreateTempDirectory()
        {
            _logger.Debug("Creating a temp directory");
            var directoryName = Path.GetRandomFileName();
            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), directoryName));
            directoryInfo.Create();
            _logger.Information("Temp directory created at {tempDirectoryPath}", directoryInfo.FullName);

            return directoryInfo;
        }

        public async ValueTask<FileInfo> WriteToFile(Stream stream, string? filePath = null, long? totalLength = null,
            IProgress<IFileSystemService.FileWriteProgress>? bytesWrittenProgress = null)
        {
            totalLength ??= 0L;
            filePath ??= Path.GetTempFileName();
            _logger.Information("Writing {totalLength} bytes to {filePath}", totalLength, filePath);

            var totalBytesRead = 0L;
            int bytesRead;
            var buffer = new byte[4 * 1024];

            await using var fs = File.OpenWrite(filePath);
            _logger.Debug($"Opened file {filePath} for writing", filePath);

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;
                bytesWrittenProgress?.Report(new IFileSystemService.FileWriteProgress((long) totalLength, totalBytesRead));
            }

            _logger.Information("Wrote {totalBytesRead}/{totalLength} bytes to file {filePath}", totalBytesRead, totalLength, filePath);

            return new FileInfo(filePath);
        }

        /*
         * Todo: Going to need to acquire some wrapper or another to set 755 perms on Linux systems
         */
        public async ValueTask<string> CreateScriptFile(string destinationDirectory, string fileName, string script)
        {
            _logger.Information("Creating a script file at {destinationDirectory} with file name {fileName} writing script:\n{script}", destinationDirectory, fileName, script);
            var destFileInfo = new FileInfo(Path.Combine(destinationDirectory, fileName + GetScriptFileExtension()));
            var destDirInfo = new DirectoryInfo(destinationDirectory);
            
            if (destDirInfo.Parent is not null && !destDirInfo.Exists) // If we're not a root directory.. 
            {
                _logger.Debug("Directory {destinationDirectory} did not exist, creating");
                destDirInfo.Create();
            }

            _logger.Verbose("Creating script file for writing");
            await using var fileWriter = new StreamWriter(destFileInfo.Create());

            _logger.Verbose("Writing script to file");
            await fileWriter.WriteAsync(script);

            destFileInfo.Refresh();

            _logger.Information("Created script file {scriptFile} of size {scriptFileSizeBytes}", destFileInfo.FullName, destFileInfo.Length);
            return destFileInfo.FullName;
        }

        public ValueTask<DirectoryInfo> CreateDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                _logger.Debug("Attempted to create directory at {directoryPath}, but the directory already exists", directoryPath);
                return ValueTask.FromResult(new DirectoryInfo(directoryPath));
            }

            _logger.Debug("Creating directory {directory}", directoryPath);
            return ValueTask.FromResult(Directory.CreateDirectory(directoryPath));
        }

        public ValueTask DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.Warning("Attempted to delete file at {filePath}, but the file did not exist", filePath);
                return ValueTask.CompletedTask;
            }

            _logger.Debug("Removing file {filePath}", filePath);
            File.Delete(filePath);

            return ValueTask.CompletedTask;
        }

        public ValueTask DeleteDirectoryRecursive(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.Warning("Attempted to delete directory at {directoryPath}, but the directory did not exist", directoryPath);
                return ValueTask.CompletedTask;
            }

            _logger.Debug("Recursively removing directory {directoryPath}", directoryPath);
            Directory.Delete(directoryPath, true);
            return ValueTask.CompletedTask;
        }

        public ValueTask DeleteDirectory(string directoryPath, bool skipIfNotEmpty = false)
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.Warning("Attempted to delete directory at {directoryPath}, but the directory did not exist", directoryPath);
                return ValueTask.CompletedTask;
            }

            if (skipIfNotEmpty && Directory.EnumerateFileSystemEntries(directoryPath).Any())
            {
                _logger.Debug("Attempted to delete directory at {directoryPath}, but the directory was not empty -- skipping", directoryPath);
                return ValueTask.CompletedTask;
            }

            _logger.Debug("Removing directory {directoryPath}", directoryPath);
            Directory.Delete(directoryPath);
            return ValueTask.CompletedTask;
        }

        public string GetScriptFileExtension()
            => CurrentPlatform.OS switch
               {
                   OS.Windows => WindowsScriptFileExtension,
                   OS.Osx     => OsxScriptFileExtension,
                   _          => LinuxScriptFileExtension,
               };
    }
}