using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Husky.Core;
using Husky.Core.Workflow.Uninstallation;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks
{
    public class HuskyContext
    {
        public string CurrentJobName { get; set; } = string.Empty;
        public string CurrentStepName { get; set; } = string.Empty;

        public Assembly InstallationAssembly { get; }

        public IUninstallOperationsList UninstallOperations { get; }

        public IReadOnlyDictionary<string, string> Variables => _variables;
        private readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);

        private readonly ILogger _logger;

        public HuskyContext(ILogger<HuskyContext> logger, IUninstallOperationsList uninstallOperationsList, Assembly installationAssembly)
        {
            _logger = logger;
            UninstallOperations = uninstallOperationsList;
            InstallationAssembly = installationAssembly;
        }

        public void SetVariable(string key, object value)
        {
            var formattedKey = FormatVariableName(key);
            _logger.LogDebug("Setting variable {key} to value {value}", key, value);
            _variables[formattedKey] = value.ToString() ?? throw new ArgumentNullException(nameof(value));
        }

        private string FormatVariableName(string variableName) => $"{CurrentJobName}.{CurrentStepName}.{variableName}";
    }
}