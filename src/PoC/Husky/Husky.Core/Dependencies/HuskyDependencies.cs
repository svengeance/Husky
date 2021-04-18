using Husky.Core.HuskyConfiguration;

// Todo: Probably should split this out. But they all look so cute together!

/*
 * NOTE: The positional properties and enums defined here follow a pattern wherein
 *       properties are suffixed with "Kind", and the property and enum name must otherwise match.
 *
 *       This is necessary to provide a pattern by which we can appropriately source generate these enums
 *       with their fully qualified names, because otherwise telling apart a string from an enum is..unobvious.
 *
 *       ..this is the bed we made and we better get comfortable. At least there aren't that many dependencies and this won't change often. Right?
 */
namespace Husky.Core.Dependencies
{
    public record DotNet(string Range, DotNet.FrameworkInstallation FrameworkInstallationKind, DotNet.RuntimeInstallation RuntimeInstallationKind = DotNet.RuntimeInstallation.Sdk): HuskyDependency(Range)
    {
        public enum RuntimeInstallation
        {
            Sdk,
            AspNet,
            Desktop,
            RuntimeOnly
        }

        public enum FrameworkInstallation
        {
            Runtime,
            Sdk
        }

    }
    public record DotNetFramework(string Range, DotNet.FrameworkInstallation Kind) : HuskyDependency(Range);

    public record Java(string Range, Java.FrameworkInstallation FrameworkInstallationKind, Java.Implementation ImplementationKind): HuskyDependency(Range)
    {
        public enum Implementation
        {
            OpenJdk,
            Oracle
        }

        public enum FrameworkInstallation
        {
            Runtime,
            Sdk
        }
    }

    public record VisualCppRedistributable(string Version): HuskyDependency(Version);
}