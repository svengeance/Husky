using System;
using System.IO;
using System.Threading.Tasks;
using Husky.Core.TaskConfiguration.Resources;
using Husky.Core.Workflow;
using Husky.Services;

namespace Husky.Tasks.Resources
{
    public class ExtractBundledResource : HuskyTask<ExtractBundledResourceOptions>
    {
        private readonly IEmbeddedResourceService _embeddedResourceService;

        public ExtractBundledResource(IEmbeddedResourceService embeddedResourceService)
        {
            _embeddedResourceService = embeddedResourceService;
        }

        protected override async Task ExecuteTask()
        {
            var availableFiles = _embeddedResourceService.ListResources(InstallationContext.InstallationAssembly, Configuration.Resources);

            if (Directory.Exists(Configuration.TargetDirectory))
            {
                if (Configuration.CleanFiles)
                    foreach (var file in Directory.EnumerateFiles(Configuration.TargetDirectory))
                        File.Delete(file);
                
                if (Configuration.CleanDirectories)
                    foreach (var dir in Directory.EnumerateDirectories(Configuration.TargetDirectory))
                        Directory.Delete(dir, true);
            }
            else
            {
                Directory.CreateDirectory(Configuration.TargetDirectory);
            }

            foreach (var file in availableFiles)
            {
                var destPath = Path.Combine(Configuration.TargetDirectory, file);
                var destDir = Path.GetDirectoryName(destPath);
                
                if (destDir != null)
                    Directory.CreateDirectory(destDir);
                
                await using var resourceStream = _embeddedResourceService.RetrieveResource(InstallationContext.InstallationAssembly, file);
                await using var fs = File.OpenWrite(destPath);
                await resourceStream.CopyToAsync(fs);
            }
        }

        protected override Task RollbackTask()
        {
            throw new NotImplementedException();
        }
    }
}