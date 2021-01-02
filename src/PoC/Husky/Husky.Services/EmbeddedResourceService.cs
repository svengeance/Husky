using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Husky.Services
{
    public interface IEmbeddedResourceService
    {
        string[] ListResources(Assembly assembly);

        IEnumerable<string> ListResources(Assembly assembly, string include);

        Stream RetrieveResource(Assembly assembly, string resourceName);
    }

    public class EmbeddedResourceService : IEmbeddedResourceService
    {
        public string[] ListResources(Assembly assembly) => assembly.GetManifestResourceNames();
        
        public IEnumerable<string> ListResources(Assembly assembly, string include)
        {
            var availableResources = ListResources(assembly);

            var matcher = new Matcher(StringComparison.InvariantCultureIgnoreCase);
            matcher.AddInclude(include);

            var matchingFiles = matcher.Match(availableResources);
            return matchingFiles.Files.Select(s => s.Path);
        }

        public Stream RetrieveResource(Assembly assembly, string resourceName)
            => assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Unable to locate resource {resourceName} from assembly {assembly.FullName}");
    }
}