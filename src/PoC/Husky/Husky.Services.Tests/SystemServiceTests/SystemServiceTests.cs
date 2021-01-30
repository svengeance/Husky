using System.Threading.Tasks;
using NUnit.Framework;

namespace Husky.Services.Tests.SystemServiceTests
{
    public class SystemServiceTests: BaseUnitTest<SystemService>
    {
        [Test]
        [Category("UnitTest")]
        public async ValueTask System_information_populates_all_possible_values()
        {
            // Arrange
            // Act
            var systemInformation = await Sut.GetSystemInformation();

            // Assert
            Assert.NotNull(systemInformation);
            Assert.NotZero(systemInformation.TotalMemoryMb);

            foreach (var drive in systemInformation.DriveInformation)
            {
                Assert.NotZero(drive.FreeSpaceMb);
                Assert.NotNull(drive.RootDirectory);
            }
        }
    }
}