using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger _logger;

        public EmbeddedResourceService(ILogger<EmbeddedResourceService> logger)
        {
            _logger = logger;
        }

        public string[] ListResources(Assembly assembly) => assembly.GetManifestResourceNames();
        
        public IEnumerable<string> ListResources(Assembly assembly, string include)
        {
            _logger.LogDebug("Locating resources in {assembly} that match pattern {includePattern}", assembly.FullName, include);
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