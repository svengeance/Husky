using System.Runtime.InteropServices;
using Husky.Core.Enums;
using SemanticVersioning;

namespace Husky.Core.Platform
{
    public interface IPlatformInformation
    {
        OS OS { get; init; }
        Architecture OSArchitecture { get; init; }
        LinuxDistribution? LinuxDistribution { get; init; }
        Version OSVersion { get; init; }
        string LongDescription { get; }
    }
}