using System.Collections.Generic;
using Husky.Core.Dependencies;
using Husky.Core.Enums;
using Husky.Dependencies.DependencyAcquisitionMethods;

namespace Husky.Dependencies.DependencyHandlers
{
    public partial class DotNetDependencyHandler
    {
        private const string PassiveInstallArgs = @"/install /passive";

        private IEnumerable<DependencyAcquisitionMethod<DotNet>> AvailableWindowsDependencies => new[]
        {
            new HttpDownloadDependencyAcquisitionMethod<DotNet>(new("5.0.1", DotNet.FrameworkInstallation.Runtime, DotNet.RuntimeInstallation.RuntimeOnly))
            {
                DownloadUrl_x64 = @"https://download.visualstudio.microsoft.com/download/pr/93095e51-be33-4b28-99c8-5ae0ebba753d/501f77f4b95d2e9c3481246a3eff9956/dotnet-runtime-5.0.1-win-x64.exe",
                DownloadUrl_x86 = @"https://download.visualstudio.microsoft.com/download/pr/f4fb5042-8134-4434-8835-499eb2f18b38/6a0d857f6f1833f5c54fbbe5ead028a7/dotnet-runtime-5.0.1-win-x86.exe",
                DownloadedFileName_x64 = @"dotnet-runtime-5.0.1-win-x64.exe",
                DownloadedFileName_x86 = @"dotnet-runtime-5.0.1-win-x86.exe",
                InstallationArguments = PassiveInstallArgs
            },
            new HttpDownloadDependencyAcquisitionMethod<DotNet>(new("5.0.1", DotNet.FrameworkInstallation.Runtime, DotNet.RuntimeInstallation.Desktop))
            {
                DownloadUrl_x64 = @"https://download.visualstudio.microsoft.com/download/pr/c6a74d6b-576c-4ab0-bf55-d46d45610730/f70d2252c9f452c2eb679b8041846466/windowsdesktop-runtime-5.0.1-win-x64.exe",
                DownloadUrl_x86 = @"https://download.visualstudio.microsoft.com/download/pr/55bb1094-db40-411d-8a37-21186e9495ef/1a045e29541b7516527728b973f0fdef/windowsdesktop-runtime-5.0.1-win-x86.exe",
                DownloadedFileName_x64 = @"windowsdesktop-runtime-5.0.1-win-x64.exe",
                DownloadedFileName_x86 = @"windowsdesktop-runtime-5.0.1-win-x86.exe",
                InstallationArguments = PassiveInstallArgs
            },
            new HttpDownloadDependencyAcquisitionMethod<DotNet>(new("5.0.1", DotNet.FrameworkInstallation.Runtime, DotNet.RuntimeInstallation.AspNet))
            {
                DownloadUrl_x64 = @"https://download.visualstudio.microsoft.com/download/pr/dff39ddb-b399-43c5-9af0-04875134ce04/1c449bb9ad4cf75ec616482854751069/dotnet-hosting-5.0.3-win.exe",
                DownloadUrl_x86 = @"https://download.visualstudio.microsoft.com/download/pr/dff39ddb-b399-43c5-9af0-04875134ce04/1c449bb9ad4cf75ec616482854751069/dotnet-hosting-5.0.3-win.exe",
                DownloadedFileName_x64 = @"dotnet-hosting-5.0.3-win.exe",
                DownloadedFileName_x86 = @"dotnet-hosting-5.0.3-win.exe",
                InstallationArguments = PassiveInstallArgs
            }
        };
        
        protected override IEnumerable<DependencyAcquisitionMethod<DotNet>> GetAvailableWindowsDependencies(DotNet dependency) => AvailableWindowsDependencies;
    }
}