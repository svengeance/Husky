using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Internal.Shared;
using Microsoft.Win32;
using Version = SemVer.Version;

namespace Husky.Core
{
    /// <summary>
    /// Represents everything about the current-running platform. In development situations this isn't particularly useful,
    /// as this information is really only desirable on client machines.
    ///
    /// Data is retrieved once on app-start, and stored forever more.
    /// </summary>
    public static class CurrentPlatform
    {
        // ReSharper disable once InconsistentNaming
        public static OS OS { get; private set; }
        
        public static Architecture OSArchitecture { get; } = RuntimeInformation.OSArchitecture;
        
        public static LinuxDistribution? LinuxDistribution { get; private set; }
        
        // ReSharper disable once InconsistentNaming
        public static Version OSVersion { get; private set; }
        
        public static string LongDescription => $"${OS} ({OSArchitecture}), {LinuxDistribution} {OSVersion} [{RuntimeInformation.OSDescription} - {RuntimeInformation.FrameworkDescription}]";

        private static OsReleaseInformation? LinuxOsRelease { get; set; }

        static CurrentPlatform()
        {
            OS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? OS.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? OS.Osx
                    : RuntimeInformation
                       .IsOSPlatform(OSPlatform.Linux)
                        ? OS.Linux
                        : throw new PlatformNotSupportedException($"{RuntimeInformation.OSDescription} is not a supported Operating System.");
        }

        // Todo: We may need to be called statically at app start so we have all this information
        internal static async Task LoadCurrentPlatformInformation()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var osInformation = await ReadOsReleaseInformation();

                if (osInformation == null)
                    return;

                // Todo: Log something about our inability to derive this distribution
                _ = Enum.TryParse<LinuxDistribution>(osInformation.Id, true, out var linuxDistribution);
                LinuxDistribution = linuxDistribution;
                
                OSVersion = new PartialVersion(osInformation.VersionId).ToZeroVersion();
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Todo: Unequivocally confirm we have read-access to this registry key. Not going to demand admin rights for something so small.
                var releaseId = Registry.LocalMachine.GetValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", string.Empty)!.ToString();
                if (!string.IsNullOrWhiteSpace(releaseId))
                    OSVersion = new PartialVersion(releaseId).ToZeroVersion();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Osx specific things
            }
            else
            {
                // Log? 
            }
        }

        private static async Task<OsReleaseInformation?> ReadOsReleaseInformation()
        {
            var osReleasePath = Path.Combine("/", "etc", "os-release");
            if (!File.Exists(osReleasePath))
                return null;

            var osReleaseDict = (await File.ReadAllLinesAsync(osReleasePath)).ToDictionary(k => k.Substring(0, k.IndexOf('=')),
                                                                                           v => v.Substring(v.IndexOf('=') + 1).Trim('"'));

            return new OsReleaseInformation(
                osReleaseDict.GetValueOrDefault("NAME", string.Empty),
                osReleaseDict.GetValueOrDefault("VERSION", string.Empty),
                osReleaseDict.GetValueOrDefault("ID", string.Empty),
                osReleaseDict.GetValueOrDefault("ID_LIKE", string.Empty),
                osReleaseDict.GetValueOrDefault("PRETTY_NAME", string.Empty),
                osReleaseDict.GetValueOrDefault("VERSION_ID", string.Empty)
            );
        }

        private record OsReleaseInformation(string Name, string Version, string Id, string IdLike, string VersionId, string PrettyName);
    }
}