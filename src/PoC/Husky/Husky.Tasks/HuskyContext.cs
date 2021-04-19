using System;
using System.Collections.Generic;
using System.Reflection;
using Husky.Core.Workflow.Uninstallation;
using Microsoft.Extensions.Logging;

namespace Husky.Tasks
{
    public class HuskyContext
    {
        public string CurrentJobName { get; set; } = string.Empty;
        public string CurrentStepName { get; set; } = string.Empty;

        public Dictionary<string, object> Variables { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public Assembly InstallationAssembly { get; }

        public IUninstallOperationsList UninstallOperations { get; }

        public string TagToExecute { get; }

        private readonly ILogger _logger;

        public HuskyContext(ILogger<HuskyContext> logger, IUninstallOperationsList uninstallOperationsList, Assembly installationAssembly, string tagToExecute)
        {
            _logger = logger;
            UninstallOperations = uninstallOperationsList;
            InstallationAssembly = installationAssembly;
            TagToExecute = tagToExecute;
        }

        public void AppendAllVariables(IEnumerable<KeyValuePair<string, object>> variables)
        {
            foreach (var (k, v) in variables)
                Variables[k] = v;
        }

        public void SetCurrentTaskVariable(string key, object value)
        {
            var formattedKey = FormatVariableName(key);
            _logger.LogDebug("Setting variable {key} to value {value}", key, value);
            Variables[formattedKey] = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string FormatVariableName(string variableName) => $"{CurrentJobName}.{CurrentStepName}.{variableName}";
    }
}