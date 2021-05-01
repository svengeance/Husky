using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Husky.Core.Dependencies;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming
namespace Husky.Dependencies.DependencyAcquisitionMethods
{
    public class HttpDownloadDependencyAcquisitionMethod<T>: DependencyAcquisitionMethod<T> where T: HuskyDependency
    {
        public string DownloadUrl { get; init; } = string.Empty;
        public string DownloadUrl_x86 { get; init; } = string.Empty;
        public string DownloadUrl_x64 { get; init; } = string.Empty;

        public string DownloadedFileName { get; init; } = string.Empty;
        public string DownloadedFileName_x86 { get; init; } = string.Empty;
        public string DownloadedFileName_x64 { get; init; } = string.Empty;

        public string InstallationArguments { get; init; } = string.Empty;

        public HttpDownloadDependencyAcquisitionMethod(T dependency): base(dependency) { }

        public string GetDownloadUrl()
            => CurrentPlatform.OSArchitecture switch
               {
                   _ when !string.IsNullOrEmpty(DownloadUrl) => DownloadUrl,
                   Architecture.X64                          => DownloadUrl_x64,
                   Architecture.X86                          => DownloadUrl_x86,
                   _                                         => throw new PlatformNotSupportedException()
               };

        public string GetDownloadFileName()
            => CurrentPlatform.OSArchitecture switch
               {
                   _ when !string.IsNullOrEmpty(DownloadedFileName) => DownloadedFileName,
                   Architecture.X64                                 => DownloadedFileName_x64,
                   Architecture.X86                                 => DownloadedFileName_x86,
                   _                                                => throw new PlatformNotSupportedException()
               };
    }
}