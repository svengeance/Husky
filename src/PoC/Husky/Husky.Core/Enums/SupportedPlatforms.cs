using System;

namespace Husky.Core.Enums
{
    [Flags]
    public enum SupportedPlatforms
    {
        Windows = 1 << 1,
        Linux = 1 << 2,
        Mac = 1 << 3,
        // ReSharper disable once InconsistentNaming
        FreeBSD = 1 << 4,
        UnixSystems = Linux | Mac | FreeBSD,
        All = Windows | Linux | Mac | FreeBSD
    }
}