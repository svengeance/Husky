using System;

namespace Husky.Core.Enums
{
    [Flags]
    public enum SupportedPlatforms
    {
        Windows = 0,
        Linux = 1 << 1,
        Mac = 1 << 2,
        // ReSharper disable once InconsistentNaming
        FreeBSD = 1 << 3,
        All = Windows | Linux | Mac | FreeBSD
    }
}