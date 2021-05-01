using System;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;

namespace Husky.Services
{
    public interface IRegistryService
    {
        void WriteKey(RegistryHive root, string path, string keyName, object value);
        void RemoveKey(string regPath);
        void RemoveKey(RegistryHive root, string path);
        void RemoveKeyValue(string regPath);
        void RemoveKeyValue(RegistryHive root, string path, string keyName);
    }

    [SupportedOSPlatform("windows")]
    public class RegistryService: IRegistryService
    {
        private readonly ILogger _logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(RegistryService));

        public void WriteKey(RegistryHive root, string path, string keyName, object value)
        {
            _logger.Information("Preparing to write to registry subkey {registryHive}/{registryPath} and key {registryKey} value {registryValue}", root, path, keyName, value);
            _logger.Verbose("Opening base key {registryHive}", root);
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);

            _logger.Verbose("Creating subkey {subKey}", path);
            using var regKey = regRoot.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);

            _logger.Verbose("Setting key {keyName} to value {value}", keyName, value);
            regKey.SetValue(keyName, value);
            
            _logger.Information("Successfully wrote {registryHive}/{registryPath}/{registryKey}:{registryValue} to registry", root, path, keyName, value);
        }

        public void RemoveKey(string regPath)
        {
            var (hive, path) = ParseRegistryKeyPath(regPath);
            RemoveKey(hive, path);
        }

        public void RemoveKey(RegistryHive root, string path)
        {
            _logger.Information("Preparing to delete registry subkey {registryHive}/{registryPath}", root, path);
            _logger.Verbose("Opening base key {registryHive}", root);
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);

            _logger.Verbose("Deleting subkey tree {subKey}", path);
            regRoot.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            
            _logger.Information("Successfully removed key from registry");
        }

        public void RemoveKeyValue(string regPath)
        {
            var (hive, path, keyName) = ParseRegistryKeyPathWithValue(regPath);
            RemoveKeyValue(hive, path, keyName);
        }

        public void RemoveKeyValue(RegistryHive root, string path, string keyName)
        {
            _logger.Information("Preparing to delete registry keyvalue {registryHive}/{registryPath}/{keyName}", root, path, keyName);
            _logger.Verbose("Opening base key {registryHive}", root);
            
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            using var regKey = regRoot.OpenSubKey(path, writable: true);
            if (regKey is null)
            {
                _logger.Warning("Tried to delete registry keyvalue {registryHive}/{registryPath}/{keyName}, but the path did not exist", root, path, keyName);
                return;
            }

            var regValue = regKey.GetValue(keyName);
            if (regValue is null)
            {
                _logger.Warning("Tried to delete registry keyvalue {registryHive}/{registryPath}/{keyName}, but the value did not exist", root, path, keyName);
                return;
            }

            _logger.Verbose("Deleting key value {subKey}/{keyValue}", path, keyName);
            regKey.DeleteValue(keyName);
            _logger.Information("Successfully removed keyvalue from registry");
        }

        private (RegistryHive hive, string path) ParseRegistryKeyPath(string regPath)
        {
            _logger.Verbose("Parsing {regPath} into a hive and path", regPath);
            var firstSlashIndex = regPath.IndexOf('\\');
            var regHive = Enum.Parse<RegistryHive>(regPath[..firstSlashIndex]);
            var keyPath = regPath[(firstSlashIndex + 1)..];
            return (regHive, keyPath);
        }

        private (RegistryHive hive, string path, string value) ParseRegistryKeyPathWithValue(string regPath)
        {
            _logger.Verbose("Parsing {regPath} into a hive and path and value", regPath);
            var (hive, keyAndValue) = ParseRegistryKeyPath(regPath);
            var keyValueSlashIndex = keyAndValue.LastIndexOf('\\');
            var keyPath = keyAndValue[..keyValueSlashIndex];
            var keyValueName = keyAndValue[(keyValueSlashIndex + 1)..];
            return (hive, keyPath, keyValueName);
        }
    }
}