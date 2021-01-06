using System.IO;
using NUnit.Framework;

namespace Husky.Tasks.Tests
{
    public abstract class BaseFileSystemTest<T>: BaseHuskyTaskTest<T>
    {
        protected DirectoryInfo _tempDirectory;

        [OneTimeSetUp]
        public void SetupDirectory()
        {
            var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirPath);
            _tempDirectory = new DirectoryInfo(tempDirPath);
        }

        [TearDown]
        public void CleanDirectory()
        {
            _tempDirectory.Delete(true);
            _tempDirectory.Create();
            _tempDirectory.Refresh();
        }

        [OneTimeTearDown]
        public void RemoveDirectory()
        {
            _tempDirectory.Refresh();

            if (_tempDirectory.Exists)
                _tempDirectory.Delete(true);
        }
    }
}