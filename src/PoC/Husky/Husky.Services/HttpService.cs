using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Husky.Services
{
    public interface IHttpService
    {
        ValueTask<FileInfo> DownloadFile(string url, string? destination = null, IProgress<IFileSystemService.FileWriteProgress>? progress = null);
        
        ValueTask<FileInfo> DownloadFile(HttpRequestMessage httpRequestMessage, string? destination = null,
            IProgress<IFileSystemService.FileWriteProgress>? progress = null);
    }
    
    public class HttpService: IHttpService
    {
        private readonly ILogger _logger;
        private readonly IFileSystemService _fileSystemService;
        private readonly HttpClient _httpClient;

        public HttpService(ILogger<HttpService> logger, IHttpClientFactory httpClientFactory, IFileSystemService fileSystemService)
        {
            _logger = logger;
            _fileSystemService = fileSystemService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public ValueTask<FileInfo> DownloadFile(string url, string? destination = null, IProgress<IFileSystemService.FileWriteProgress>? progress = null)
            => DownloadFile(new HttpRequestMessage(HttpMethod.Get, url), destination, progress);

        public async ValueTask<FileInfo> DownloadFile(HttpRequestMessage httpRequestMessage, string? destination = null,
            IProgress<IFileSystemService.FileWriteProgress>? progress = null)
        {
            _logger.LogInformation("Attempting to download file from {requestUrl} to path {destinationFilePath}", httpRequestMessage.RequestUri, destination);
            var response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
            _logger.LogDebug("Got response from server, status code {response} - {reason}", response.StatusCode, response.ReasonPhrase);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;
            var contentStream = await response.Content.ReadAsStreamAsync();

            return await _fileSystemService.WriteToFile(contentStream, destination, contentLength, progress);
        }
    }
}