using System.Runtime.InteropServices;
using Husky.Core.Enums;

namespace Husky.Installer.Extensions
{
    public static class SupportedPlatformExtensions
    {
        public static bool IsCurrentPlatformSupported(this SupportedPlatforms supportedPlatforms)
            => supportedPlatforms switch
               {
                   _ when (supportedPlatforms & SupportedPlatforms.Windows) != 0 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => true,
                   _ when (supportedPlatforms & SupportedPlatforms.Mac) != 0 && RuntimeInformation.IsOSPlatform(OSPlatform.OSX)         => true,
                   _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux)                            => true,
                   _                                                                                   => false
               };
    }
}