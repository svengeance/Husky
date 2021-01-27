using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;

// Todo: Probably should split this out. But they all look so cute together!
namespace Husky.Core.Dependencies
{
    public record DotNet(string Range, FrameworkInstallationType FrameworkType, DotNet.RuntimeKind Kind = DotNet.RuntimeKind.Sdk): HuskyDependency(Range)
    {
        public enum RuntimeKind
        {
            Sdk,
            AspNet,
            Desktop,
            RuntimeOnly
        }
    }
    public record DotNetFramework(string Range, FrameworkInstallationType Type) : HuskyDependency(Range);

    public record Java(string Range, FrameworkInstallationType Type, Java.Implementation JavaImplementation): HuskyDependency(Range)
    {
        public enum Implementation { OpenJdk, Oracle }
    }

    public record VisualCppRedistributable(string Version): HuskyDependency(Version);
}