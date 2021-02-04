﻿using Microsoft.Win32;

namespace Husky.Services
{
    public interface IRegistryService
    {
        void WriteKey(RegistryHive root, string path, string keyName, object value);
    }
    
    public class RegistryService: IRegistryService
    {
        public void WriteKey(RegistryHive root, string path, string keyName, object value)
        {
            using var regRoot = RegistryKey.OpenBaseKey(root, RegistryView.Default);
            var regKey = regRoot.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryOptions.None);
            regKey.SetValue(keyName, value);
        }
    }
}