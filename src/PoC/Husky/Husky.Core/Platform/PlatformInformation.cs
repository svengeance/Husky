using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Husky.Core.Enums;
using Husky.Internal.Shared;
using Microsoft.Win32;
using Version = SemVer.Version;

// ReSharper disable InconsistentNaming
namespace Husky.Core.Platform
{
    public class PlatformInformation: IPlatformInformation
    {
        public OS OS { get; init; }
        public Architecture OSArchitecture { get; init; }
        public LinuxDistribution? LinuxDistribution { get; init; }
        public Version OSVersion { get; init; } = new(0, 0, 0);
        
        public string LongDescription => $"${OS} ({OSArchitecture}), {LinuxDistribution} {OSVersion} [{RuntimeInformation.OSDescription} - {RuntimeInformation.FrameworkDescription}]";

        internal static IPlatformInformation LoadCurrentPlatformInformation()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? LoadWindowsPlatformInformation()
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? LoadLinuxPlatformInformation()
                    : throw new NotSupportedException($"Unidentified/Not supported platform detected: {RuntimeInformation.OSDescription}");

        [SupportedOSPlatform("windows")]
        private static IPlatformInformation LoadWindowsPlatformInformation()
        {
            Version? osVersion = null;
            var releaseId = Registry.LocalMachine.GetValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", string.Empty)!.ToString();
            if (!string.IsNullOrWhiteSpace(releaseId))
                osVersion = new PartialVersion(releaseId).ToZeroVersion();

            return new PlatformInformation
            {
                OS = OS.Windows,
                OSVersion = osVersion ?? new(0, 0, 0),
                OSArchitecture = RuntimeInformation.OSArchitecture
            };
        }

        private static IPlatformInformation LoadLinuxPlatformInformation()
        {
            var osInformation = ReadOsReleaseInformation();

            // Todo: Log something about our inability to derive this distribution
            _ = Enum.TryParse<LinuxDistribution>(osInformation.Id, true, out var linuxDistribution);
            var osVersion = new PartialVersion(osInformation.VersionId).ToZeroVersion();

            return new PlatformInformation
            {
                LinuxDistribution = linuxDistribution,
                OS = OS.Linux,
                OSVersion = osVersion,
                OSArchitecture = RuntimeInformation.OSArchitecture
            };
        }

        private IPlatformInformation? LoadOsxPlatformInformation() => throw new NotImplementedException("OSX not yet supported");

        private static OsReleaseInformation ReadOsReleaseInformation()
        {
            var osReleasePath = Path.Combine("/", "etc", "os-release");
            if (!File.Exists(osReleasePath))
                throw new PlatformNotSupportedException("Unable to determine Linux system information from os-release");

            var osReleaseDict = File.ReadAllLines(osReleasePath).ToDictionary(k => k.Substring(0, k.IndexOf('=')),
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