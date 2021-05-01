using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

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
        private static readonly HttpClient HttpClient = new();

        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(RegistryService));
        private readonly IFileSystemService _fileSystemService;

        public HttpService(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        public ValueTask<FileInfo> DownloadFile(string url, string? destination = null, IProgress<IFileSystemService.FileWriteProgress>? progress = null)
            => DownloadFile(new HttpRequestMessage(HttpMethod.Get, url), destination, progress);

        public async ValueTask<FileInfo> DownloadFile(HttpRequestMessage httpRequestMessage, string? destination = null,
            IProgress<IFileSystemService.FileWriteProgress>? progress = null)
        {
            _logger.Information("Attempting to download file from {requestUrl} to path {destinationFilePath}", httpRequestMessage.RequestUri, destination);
            var response = await HttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
            _logger.Debug("Got response from server, status code {response} - {reason}", response.StatusCode, response.ReasonPhrase);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;
            var contentStream = await response.Content.ReadAsStreamAsync();

            return await _fileSystemService.WriteToFile(contentStream, destination, contentLength, progress);
        }
    }
}