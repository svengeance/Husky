using System;
using System.Collections.Generic;

namespace Husky.Tasks
{
    public class InstallationContext
    {
        private Dictionary<string, string> _variables { get; } = new();

        public IReadOnlyDictionary<string, string> Variables => _variables;
        
        public void SetVariable(string key, object value) => _variables[key] = value.ToString() ?? throw new ArgumentNullException(nameof(value));
    }
}