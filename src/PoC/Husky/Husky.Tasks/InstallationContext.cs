using System;
using System.Collections.Generic;
using System.Reflection;

namespace Husky.Tasks
{
    public class InstallationContext
    {
        public string CurrentJobName { get; set; } = string.Empty;
        public string CurrentStepName { get; set; } = string.Empty;

        public Assembly InstallationAssembly { get; }

        public IReadOnlyDictionary<string, string> Variables => _variables;

        private readonly Dictionary<string, string> _variables = new();

        public InstallationContext(Assembly installationAssembly)
        {
            InstallationAssembly = installationAssembly;
        }

        public void SetVariable(string key, object value) => _variables[FormatVariableName(key)] = value.ToString() ?? throw new ArgumentNullException(nameof(value));

        private string FormatVariableName(string variableName) => $"{CurrentJobName}.{CurrentStepName}.{variableName}";
    }
}