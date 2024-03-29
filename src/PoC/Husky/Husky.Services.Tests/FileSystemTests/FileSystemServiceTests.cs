﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Husky.Internal.Shared;
using NUnit.Framework;

namespace Husky.Services.Tests.FileSystemTests
{
    public class FileSystemServiceTests: BaseIntegrationTest<FileSystemService>
    {
        [Test]
        [Category("IntegrationTest")]
        public void Create_temp_directory_creates_and_returns_real_directory()
        {
            // Arrange
            // Act
            var tempDir = Sut.CreateTempDirectory();

            // Assert
            DirectoryAssert.Exists(tempDir);

            var systemTempDirectory = Path.GetTempPath();
            StringAssert.StartsWith(systemTempDirectory, tempDir.FullName);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Write_to_file_writes_all_content()
        {
            // Arrange
            var expectedContent = "WARNING. MILITARY SOFTWARE DETECTED. TOP SECRET CLEARANCE REQUIRED.";
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            // Act
            var createdFile = await Sut.WriteToFile(stream);

            // Assert
            FileAssert.Exists(createdFile);

            var fileContents = await File.ReadAllTextAsync(createdFile.FullName);
            Assert.AreEqual(expectedContent, fileContents);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Write_to_file_writes_all_content_to_defined_file_path()
        {
            // Arrange
            var expectedContent = "Kirov Reporting.";
            var expectedFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent));

            // Act
            var createdFile = await Sut.WriteToFile(stream, expectedFile);

            // Assert
            FileAssert.Exists(createdFile);
            Assert.AreEqual(expectedFile, createdFile.FullName);

            var fileContents = await File.ReadAllTextAsync(createdFile.FullName);
            Assert.AreEqual(expectedContent, fileContents);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Write_to_file_raises_appropriate_events()
        {
            // Arrange
            const int numberOfBlockEnumerations = 5;
            const int expectedBlockSize = 4 * 1024;
            const long contentLength = expectedBlockSize * numberOfBlockEnumerations;
            var fileContent = new byte[contentLength];
            Array.Fill(fileContent, (byte) 128);

            await using var stream = new MemoryStream(fileContent);
            var progressCount = 0;
            long bytesWritten = 0;
            var reportedLengths = new List<long>();
            var reportedPercent = string.Empty;

            var progress = new SequentialBlockingProgressHandler<IFileSystemService.FileWriteProgress>(p =>
            {
                progressCount++;
                bytesWritten = p.BytesWritten;
                reportedLengths.Add(p.BytesLength);
                reportedPercent = p.ProgressPercent;
            });

            var expectedProgressCount = (int) Math.Round((decimal) contentLength / expectedBlockSize, MidpointRounding.ToPositiveInfinity);
            var expectedReportedLengths = Enumerable.Range(0, expectedProgressCount).Select(_ => contentLength);

            // Act
            _ = await Sut.WriteToFile(stream, totalLength: contentLength, bytesWrittenProgress: progress);
            await progress.DisposeAsync();
            
            // Assert
            Assert.AreEqual(expectedProgressCount, progressCount);
            Assert.AreEqual(contentLength, bytesWritten);
            Assert.AreEqual(reportedPercent, "100%");
            CollectionAssert.AreEqual(expectedReportedLengths, reportedLengths);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Create_directory_creates_directory()
        {
            // Arrange
            var newDirectoryName = Guid.NewGuid().ToString();

            // Act
            var createdDir = await Sut.CreateDirectory(Path.Combine(Path.GetTempPath(), newDirectoryName));

            // Assert
            DirectoryAssert.Exists(createdDir);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Create_directory_returns_directory_if_it_already_exists()
        {
            // Arrange
            var newDirectoryName = Guid.NewGuid().ToString();
            var newDirectoryPath = Path.Combine(Path.GetTempPath(), newDirectoryName);
            Directory.CreateDirectory(newDirectoryPath);

            // Act
            var createdDir = await Sut.CreateDirectory(newDirectoryPath);

            // Assert
            DirectoryAssert.Exists(createdDir);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Delete_file_deletes_file()
        {
            // Arrange
            var helplessFile = new FileInfo(Path.GetTempFileName());

            // Act
            await Sut.DeleteFile(helplessFile.FullName);

            // Assert
            FileAssert.DoesNotExist(helplessFile);
        }

        [Test]
        [Category("IntegrationTest")]
        public void Delete_file_does_not_throw_when_file_does_not_exist()
        {
            // Arrange
            var sneakyNonExistantFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".realFile");

            // Act
            Task DeleteFile() => Sut.DeleteFile(sneakyNonExistantFile).AsTask();

            // Assert
            FileAssert.DoesNotExist(sneakyNonExistantFile); // "There's a chance"
            Assert.DoesNotThrowAsync(DeleteFile);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Delete_directory_deletes_empty_directory()
        {
            // Arrange
            var helplessDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            // Act
            await Sut.DeleteDirectory(helplessDirectory.FullName);

            // Assert
            DirectoryAssert.DoesNotExist(helplessDirectory);
        }

        [Test]
        [Category("IntegrationTest")]
        public void Delete_directory_does_not_throw_when_directory_does_not_exist()
        {
            // Arrange
            var sneakyNonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".realFile");

            // Act
            Task DeleteDirectory() => Sut.DeleteDirectory(sneakyNonExistentDirectory).AsTask();

            // Assert
            DirectoryAssert.DoesNotExist(sneakyNonExistentDirectory); // "There's a chance"
            Assert.DoesNotThrowAsync(DeleteDirectory);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Delete_directory_does_not_delete_directory_or_throw_when_directory_is_not_empty_when_skip_parameter_is_specified()
        {
            // Arrange
            var directoryWithData = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            var filePreventingDirectoryFromBeingDeleted = new FileInfo(Path.Combine(directoryWithData.FullName, "block.txt"));
            await File.WriteAllTextAsync(filePreventingDirectoryFromBeingDeleted.FullName, "You'll never delete me alive!");

            // Act
            await Sut.DeleteDirectory(directoryWithData.FullName, skipIfNotEmpty: true);

            // Assert
            DirectoryAssert.Exists(directoryWithData);
            FileAssert.Exists(filePreventingDirectoryFromBeingDeleted);
        }

        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Delete_directory_recursive_removes_all_files_and_folders()
        {
            // Arrange
            var directoryWithData = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            var helplessFile = new FileInfo(Path.Combine(directoryWithData.FullName, "blockAttempt.txt"));
            var helplessDirectory = Directory.CreateDirectory(Path.Combine(directoryWithData.FullName, "SpanishInquisition"));
            var anotherHelplessFile = new FileInfo(Path.Combine(helplessDirectory.FullName, "anotherFutileBlockAttempt.txt"));
            await File.WriteAllTextAsync(helplessFile.FullName, "Any last words?");
            await File.WriteAllTextAsync(anotherHelplessFile.FullName, "Fergulous");

            // Act
            await Sut.DeleteDirectoryRecursive(directoryWithData.FullName);

            // Assert
            DirectoryAssert.DoesNotExist(directoryWithData);
            FileAssert.DoesNotExist(helplessFile);
            FileAssert.DoesNotExist(anotherHelplessFile);
        }

        [Test]
        [Category("IntegrationTest")]
        public void Delete_directory_recursive_does_not_throw_when_directory_does_not_exist()
        {
            // Arrange
            var sneakyNonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".realFile");

            // Act
            Task DeleteDirectoryRecursive() => Sut.DeleteDirectoryRecursive(sneakyNonExistentDirectory).AsTask();

            // Assert
            DirectoryAssert.DoesNotExist(sneakyNonExistentDirectory); // "There's a chance"
            Assert.DoesNotThrowAsync(DeleteDirectoryRecursive);
        }
    }
}