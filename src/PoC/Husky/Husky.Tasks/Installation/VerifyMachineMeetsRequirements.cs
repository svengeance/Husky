using System;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskConfiguration.Installation;
using Husky.Services;

namespace Husky.Tasks.Installation
{
    public class VerifyMachineMeetsRequirements: HuskyTask<VerifyMachineMeetsRequirementsOptions>
    {
        private readonly ISystemService _systemService;
        private readonly ClientMachineRequirementsConfiguration _requirements;

        public VerifyMachineMeetsRequirements(ISystemService systemService, ClientMachineRequirementsConfiguration requirements)
        {
            _systemService = systemService;
            _requirements = requirements;
        }
        
        protected override async ValueTask ExecuteTask()
        {
            var systemInformation = await _systemService.GetSystemInformation();

            if (_requirements.SupportedOperatingSystems.Length > 0 && !_requirements.SupportedOperatingSystems.Contains(CurrentPlatform.OS))
            {
                var supportedOsString = string.Join(", ", _requirements.SupportedOperatingSystems);
                throw new NotSupportedException($"Machine's Operating System is unsupported. Required: {supportedOsString}, Detected: {CurrentPlatform.OS}");
            }
            
            if (_requirements.LinuxDistribution != LinuxDistribution.Unknown && _requirements.LinuxDistribution != CurrentPlatform.LinuxDistribution)
                HandleIssue($"Machine's Linux Distribution is not supported. Required: {_requirements.LinuxDistribution}, Detected: {CurrentPlatform.LinuxDistribution}");

            if (systemInformation.TotalMemoryMb < _requirements.RamMb)
                HandleIssue($"Machine has less than the required amount of memory. Required: {_requirements.RamMb}, Detected: {systemInformation.TotalMemoryMb}");
            
            if (systemInformation.DriveInformation.All(a => a.FreeSpaceMb < _requirements.FreeSpaceMb))
                HandleIssue($"Machine has less than the required amount of free space. Required: {_requirements.FreeSpaceMb}");

            if (_requirements.OsVersion?.IsSatisfied(CurrentPlatform.OSVersion) == false)
                HandleIssue($"Machine's Operating System does does not meet the required version. Required: {_requirements.OsVersion}, Detected: {CurrentPlatform.OSVersion}");
        }

        private void HandleIssue(string message)
        {
            if (Configuration.WarnInsteadOfHalt) // Todo: Replace with UI message once UI is available
                Console.WriteLine(message);
            else
                throw new ApplicationException("Failed to meet requirements");
        }

        protected override ValueTask RollbackTask() => ValueTask.CompletedTask;
    }
}