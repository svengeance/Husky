using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Husky.Services
{
    public interface IRegistryService
    {
        void WriteKey(RegistryHive root, string path, string keyName, object value);
        void RemoveSubKey(RegistryHive root, string path);
    }
    
    public class RegistryService: IRegistryService
    {
        private readonly ILogger _logger;

        public RegistryService(ILogger<RegistryService> logger)
        {
            _logger = logger;
        }

        public void WriteKey(RegistryHive root, string path, string keyName, object value)
        {
            _logger.LogInformation("Preparing to write to registry subkey {registryHive}/{registryPath} and key {registryKey} value {registryValue}", root, path, keyName, value);
            _logger.LogTrace("Opening base key {registryHive}", root);
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);

            _logger.LogTrace("Creating subkey {subKey}", path);
            var regKey = regRoot.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);

            _logger.LogTrace("Setting key {keyName} to value {value}", keyName, value);
            regKey.SetValue(keyName, value);
            
            _logger.LogInformation("Successfully wrote to registry");
        }

        public void RemoveSubKey(RegistryHive root, string path)
        {
            _logger.LogInformation("Preparing to delete registry subkey {registryHive}/{registryPath}", root, path);
            _logger.LogTrace("Opening base key {registryHive}", root);
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            
            _logger.LogTrace("Deleting subkey tree {subKey}", path);
            regRoot.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            
            _logger.LogInformation("Successfully removed key from registry");
        }
    }
}