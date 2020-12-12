using System;
using System.Collections.Generic;
using System.Reflection;

namespace Husky.Installer
{
    public class InstallationConfiguration
    {
        public IEnumerable<Assembly> ResolveModulesFromAssemblies { get; set; } = Array.Empty<Assembly>();
    }
}