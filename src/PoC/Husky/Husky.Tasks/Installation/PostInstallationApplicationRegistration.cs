using System;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskConfiguration.Installation;
using Husky.Internal.Shared;
using Husky.Services;
using Microsoft.Win32;

using static Husky.Core.HuskyConstants.RegistryKeys;

namespace Husky.Tasks.Installation
{
    public class PostInstallationApplicationRegistration: HuskyTask<PostInstallationApplicationRegistrationConfiguration>
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


            // Todo: Uninstaller, https://github.com/svengeance/Husky/issues/17
            //WriteUninstallKey(UninstallKeyNames.UninstallString, !ImplementMe);
            //WriteUninstallKey(UninstallKeyNames.QuietUninstallString, !ImplementMe);
            return ValueTask.CompletedTask;
        }

        // Todo: Support HKCU and HKLM https://github.com/svengeance/Husky/issues/11
        private void WriteUninstallKey(string key, object value)
            => _registryService.WriteKey(RegistryHive.LocalMachine, HuskyConstants.RegistryKeys.AppUninstalls.RootKey, key, value);

        protected override ValueTask RollbackTask() => throw new System.NotImplementedException();
    }
}