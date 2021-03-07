using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Husky.Core.Enums;
using Version = SemVer.Version;

namespace Husky.Core.Platform
{
    /// <summary>
    /// Represents everything about the current-running platform. In development situations this isn't particularly useful,
    /// as this information is really only desirable on client machines.
    ///
    /// Data is retrieved once on app-start, and stored forever more.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class CurrentPlatform
    {
        public static OS OS => PlatformInformation.OS;

        public static Architecture OSArchitecture => PlatformInformation.OSArchitecture;

        public static LinuxDistribution? LinuxDistribution => PlatformInformation.LinuxDistribution;

        public static Version OSVersion => PlatformInformation.OSVersion;
        
        public static string LongDescription => $"${OS} ({OSArchitecture}), {LinuxDistribution} {OSVersion} [{RuntimeInformation.OSDescription} - {RuntimeInformation.FrameworkDescription}]";

        private static IPlatformInformation PlatformInformation { get; set; }

        static CurrentPlatform() => PlatformInformation = Platform.PlatformInformation.LoadCurrentPlatformInformation();

        // A bit of nasty business here that lets us override and mock out this static class.
        internal static void LoadPlatformInformation(IPlatformInformation platformInformation) => PlatformInformation = platformInformation;
    }
}