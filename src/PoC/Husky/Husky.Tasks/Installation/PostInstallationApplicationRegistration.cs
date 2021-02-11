using System;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Internal.Shared;
using Husky.Services;
using Microsoft.Win32;

using static Husky.Core.HuskyConstants.RegistryKeys;

namespace Husky.Tasks.Installation
{
    public class PostInstallationApplicationRegistration: HuskyTask<PostInstallationApplicationRegistrationOptions>
    {
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly AuthorConfiguration _authorConfiguration;
        private readonly InstallationConfiguration _installationConfiguration;
        private readonly IRegistryService _registryService;

        public PostInstallationApplicationRegistration(ApplicationConfiguration applicationConfiguration,
            AuthorConfiguration authorConfiguration,
            InstallationConfiguration installationConfiguration,
            IRegistryService registryService)
        {
            _applicationConfiguration = applicationConfiguration;
            _authorConfiguration = authorConfiguration;
            _installationConfiguration = installationConfiguration;
            _registryService = registryService;
        }
        
        protected override ValueTask ExecuteTask()
        {
            if (CurrentPlatform.OS != OS.Windows || !_installationConfiguration.AddToRegistry)
                return ValueTask.CompletedTask;

            WriteUninstallKey(AppUninstalls.DisplayName, _applicationConfiguration.Name);
            WriteUninstallKey(AppUninstalls.Publisher, _authorConfiguration.Publisher);
            WriteUninstallKey(AppUninstalls.Comments, _applicationConfiguration.Description);

            WriteUninstallKey(AppUninstalls.InstallDate, DateTime.Today.ToString("yyyyMMdd"));
            WriteUninstallKey(AppUninstalls.InstallLocation, _applicationConfiguration.InstallDirectory);
            WriteUninstallKey(AppUninstalls.DisplayVersion, _applicationConfiguration.Version);
                
            /*
             * Todo: Reassess at a later time - these Version properties appear to be a way for
             *       the glorious Windows Installer to drive its updates, and should not be used. Could. But maybe should not be.
             */ 
            //var productVersion = new PartialVersion(_applicationConfiguration.Version);
            //WriteUninstallKey(AppUninstalls.VersionMajor, productVersion.Minor ?? 0);
            //WriteUninstallKey(AppUninstalls.VersionMinor, productVersion.Major ?? 0);
            //WriteUninstallKey(AppUninstalls.Version, productVersion.ToString());
                
            WriteUninstallKey(AppUninstalls.HelpLink, _applicationConfiguration.SupportUrl);
            WriteUninstallKey(AppUninstalls.HelpTelephone, _applicationConfiguration.SupportTelephone);
            WriteUninstallKey(AppUninstalls.URLInfoAbout, _applicationConfiguration.AboutUrl);

            WriteUninstallKey(AppUninstalls.NoModify, _installationConfiguration.AllowModify ? 0 : 1);
            WriteUninstallKey(AppUninstalls.NoRepair, _installationConfiguration.AllowRepair ? 0 : 1);
            WriteUninstallKey(AppUninstalls.NoRemove, _installationConfiguration.AllowRemove ? 0 : 1);

            // Todo: GH #17 && GH #12 Uninstaller and dynamic resolution of variables (such as "What directory did I install to?")
            //WriteUninstallKey(UninstallKeyNames.UninstallString, !ImplementMe);
            //WriteUninstallKey(UninstallKeyNames.QuietUninstallString, !ImplementMe);
            return ValueTask.CompletedTask;
        }

        protected override ValueTask RollbackTask()
        {
            _registryService.RemoveSubKey(RegistryHive.LocalMachine, GetApplicationUninstallationRootKey());
            return ValueTask.CompletedTask;
        }

        private string GetApplicationUninstallationRootKey() => @$"{AppUninstalls.RootKey}\{_applicationConfiguration.Name}_Husky";

        // Todo: GH #11 - Support HKCU and HKLM
        private void WriteUninstallKey(string key, object value)
            => _registryService.WriteKey(RegistryHive.LocalMachine, GetApplicationUninstallationRootKey(), key, value);
    }
}