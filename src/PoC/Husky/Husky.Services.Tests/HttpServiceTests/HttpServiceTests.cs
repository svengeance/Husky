using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Husky.Services.Tests.HttpServiceTests
{
    public class HttpServiceTests: BaseIntegrationTest<HttpService>
    {
        [Test]
        [Category("IntegrationTest")]
        public async ValueTask Http_service_downloads_and_returns_full_response()
        {
            // Arrange
            var urlToDownload = "https://gist.githubusercontent.com/svengeance/4d449c77383db391e419a91cc50982b2/raw/edbfe20416c333b1286fde47f90fa9716787e3f1/Test_Contents.txt";
            var http = new HttpClient();
            var expectedContent = await http.GetStringAsync(urlToDownload);
            
            // Act
            var savedFile = await Sut.DownloadFile(urlToDownload);

            // Assert
            FileAssert.Exists(savedFile);

            var fileContents = await File.ReadAllTextAsync(savedFile.FullName);
            Assert.AreEqual(expectedContent, fileContents);
        }
    }
}