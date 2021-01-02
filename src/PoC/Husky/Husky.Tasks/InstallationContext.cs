using System;
using System.Collections.Generic;
using System.Reflection;

namespace Husky.Tasks
{
    public class InstallationContext
    {
        public Assembly InstallationAssembly { get; }

        private readonly Dictionary<string, string> _variables = new();

        public IReadOnlyDictionary<string, string> Variables => _variables;

        public InstallationContext(Assembly installationAssembly)
        {
            InstallationAssembly = installationAssembly;
        }
        
        public void SetVariable(string key, object value) => _variables[key] = value.ToString() ?? throw new ArgumentNullException(nameof(value));
    }
}