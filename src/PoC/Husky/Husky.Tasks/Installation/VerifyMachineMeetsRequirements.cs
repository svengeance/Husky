using System;
using System.Linq;
using System.Threading.Tasks;
using Husky.Core.Enums;
using Husky.Core.HuskyConfiguration;
using Husky.Core.Platform;
using Husky.Core.TaskOptions.Installation;
using Husky.Services;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks.Installation
{
    public class VerifyMachineMeetsRequirements: HuskyTask<VerifyMachineMeetsRequirementsOptions>
    {
        private readonly ILogger _logger;
        private readonly ISystemService _systemService;
        private readonly ClientMachineRequirementsConfiguration _requirements;

        public VerifyMachineMeetsRequirements(ILogger<VerifyMachineMeetsRequirements> logger, ISystemService systemService, ClientMachineRequirementsConfiguration requirements)
        {
            _logger = logger;
            _systemService = systemService;
            _requirements = requirements;
        }
        
        protected override async ValueTask ExecuteTask()
        {
            var systemInformation = await _systemService.GetSystemInformation();

            _logger.LogTrace("Checking OS Requirements");
            if (_requirements.SupportedOperatingSystems.Length > 0 && !_requirements.SupportedOperatingSystems.Contains(CurrentPlatform.OS))
            {
                var supportedOsString = string.Join(", ", _requirements.SupportedOperatingSystems);
                throw new NotSupportedException($"Machine's Operating System is unsupported. Required: {supportedOsString}, Detected: {CurrentPlatform.OS}");
            }
            
            _logger.LogTrace("Checking linux distribution");
            if (_requirements.LinuxDistribution != LinuxDistribution.Unknown && _requirements.LinuxDistribution != CurrentPlatform.LinuxDistribution)
                HandleIssue($"Machine's Linux Distribution is not supported. Required: {_requirements.LinuxDistribution}, Detected: {CurrentPlatform.LinuxDistribution}");

            _logger.LogTrace("Checking memory requirements");
            if (systemInformation.TotalMemoryMb < _requirements.MemoryMb)
                HandleIssue($"Machine has less than the required amount of memory. Required: {_requirements.MemoryMb}, Detected: {systemInformation.TotalMemoryMb}");
            
            _logger.LogTrace("Checking hard disk requirements");
            if (systemInformation.DriveInformation.All(a => a.FreeSpaceMb < _requirements.FreeSpaceMb))
                HandleIssue($"Machine has less than the required amount of free space. Required: {_requirements.FreeSpaceMb}");

            _logger.LogTrace("Checking OS Version");
            if (_requirements.OsVersion?.IsSatisfied(CurrentPlatform.OSVersion) == false)
                HandleIssue($"Machine's Operating System does does not meet the required version. Required: {_requirements.OsVersion}, Detected: {CurrentPlatform.OSVersion}");
        }

        private void HandleIssue(string message)
        {
            if (Configuration.WarnInsteadOfHalt) // Todo: Replace with UI message once UI is available
                _logger.LogWarning("Machine requirements triggered a warning: {warning}", message);
            else
                throw new ApplicationException($"Failed to meet requirements: {message}");
        }
    }
}