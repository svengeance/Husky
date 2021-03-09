using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Core.Workflow.Uninstallation;
using Husky.Services;
using Microsoft.Win32;

using static Husky.Core.HuskyConstants.RegistryKeys;

namespace Husky.Tasks.Installation
{
    [SupportedOSPlatform("windows")]
    public class PostInstallationApplicationRegistration: HuskyTask<PostInstallationApplicationRegistrationOptions>
    {
        private readonly ApplicationConfiguration _applicationConfiguration;
        private readonly AuthorConfiguration _authorConfiguration;
        private readonly InstallationConfiguration _installationConfiguration;
        private readonly IRegistryService _registryService;

        private readonly string _applicationRootKey;

        public PostInstallationApplicationRegistration(ApplicationConfiguration applicationConfiguration,
            AuthorConfiguration authorConfiguration,
            InstallationConfiguration installationConfiguration,
            IRegistryService registryService)
        {
            _applicationConfiguration = applicationConfiguration;
            _authorConfiguration = authorConfiguration;
            _installationConfiguration = installationConfiguration;
            _registryService = registryService;
            
            _applicationRootKey = @$"{AppUninstalls.RootKey}\{_applicationConfiguration.Name}_Husky";
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

            var applicationRootKeyWithHive = @$"{RegistryHive.LocalMachine}\{_applicationRootKey}";
            HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.RegistryKey, applicationRootKeyWithHive);
            return ValueTask.CompletedTask;
        }

        // Todo: GH #11 - Support HKCU and HKLM
        private void WriteUninstallKey(string key, object value)
        {
            _registryService.WriteKey(RegistryHive.LocalMachine, _applicationRootKey, key, value);
            var fullRegPath = string.Join('\\', RegistryHive.LocalMachine.ToString(), _applicationRootKey, key);
            HuskyContext.UninstallOperations.AddEntry(UninstallOperationsList.EntryKind.RegistryValue, fullRegPath);
        }
    }
}