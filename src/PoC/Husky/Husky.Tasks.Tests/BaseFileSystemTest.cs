using System.IO;
using NUnit.Framework;

namespace Husky.Tasks.Tests
{
    public abstract class BaseFileSystemTest<T>: BaseHuskyTaskIntegrationTest<T> where T: class
    {
        protected DirectoryInfo TempDirectory = null!;

        [OneTimeSetUp]
        public void SetupDirectory()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirPath);
            TempDirectory = new DirectoryInfo(tempDirPath);
        }

        [TearDown]
        public void CleanDirectory()
        {
            TempDirectory.Delete(true);
            TempDirectory.Create();
            TempDirectory.Refresh();
        }

        [OneTimeTearDown]
        public void RemoveDirectory()
        {
            TempDirectory.Refresh();

            if (TempDirectory.Exists)
                TempDirectory.Delete(true);
        }
    }
}