using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Husky.Services
{
    public interface IHttpService
    {
        ValueTask<FileInfo> DownloadFile(string url, string? destination = null, IProgress<FileSystemService.FileWriteProgress>? progress = null);
        
        ValueTask<FileInfo> DownloadFile(HttpRequestMessage httpRequestMessage, string? destination = null,
            IProgress<FileSystemService.FileWriteProgress>? progress = null);
    }
    
    public class HttpService: IHttpService
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly HttpClient _httpClient;

        public HttpService(IHttpClientFactory httpClientFactory, IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public ValueTask<FileInfo> DownloadFile(string url, string? destination = null, IProgress<FileSystemService.FileWriteProgress>? progress = null)
            => DownloadFile(new HttpRequestMessage(HttpMethod.Get, url), destination, progress);

        public async ValueTask<FileInfo> DownloadFile(HttpRequestMessage httpRequestMessage, string? destination = null,
            IProgress<FileSystemService.FileWriteProgress>? progress = null)
        {
            // Todo: Log successes & failures
            var response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;
            var contentStream = await response.Content.ReadAsStreamAsync();

            return await _fileSystemService.WriteToFile(contentStream, destination, contentLength, progress);
        }
    }
}