using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Husky.Core.Workflow.Uninstallation
{
    // A shim during uninstall operations such that executing user-defined uninstall operations do not themselves append to the file

    // Keeping this class as flat as possible to be kind to serialization & compression
    public sealed class UninstallOperationsList: IUninstallOperationsList
    {
        [JsonInclude]
        public HashSet<string> FilesToRemove { private get; init; } = new();

        [JsonInclude]
        public HashSet<string> DirectoriesToRemove { private get; init; } = new();

        [JsonInclude]
        public HashSet<string> RegistryValuesToRemove { private get; init; } = new();

        [JsonInclude]
        public HashSet<string> RegistryKeysToRemove { private get; init; } = new();

        private readonly OperationsListVersion _version;
        private readonly string _filePath;

        private const byte Newline = 0xA;

        private UninstallOperationsList(string filePath, OperationsListVersion version)
        {
            _filePath = filePath;
            _version = version;
        }

        public static async ValueTask<IUninstallOperationsList> CreateOrRead(string filePath)
        {
            if (File.Exists(filePath))
                return await ReadFromJson(filePath);

            var newUninstallOpsList = new UninstallOperationsList(filePath, OperationsListVersion.v1);
            var parentDirectory = new FileInfo(filePath).DirectoryName ?? throw new ArgumentException($"Unable to locate directory for uninstall file at {filePath}");

            Directory.CreateDirectory(parentDirectory);
            File.Create(filePath).Close();
            await newUninstallOpsList.Flush();

            return newUninstallOpsList;
        }

        public void AddEntry(EntryKind kind, string entry) => GetListFromKind(kind).Add(entry);

        public IEnumerable<string> ReadEntries(EntryKind kind) => GetListFromKind(kind);

        public async Task Flush()
        {
            await using var fs = File.Open(_filePath, FileMode.Truncate);
            fs.WriteByte((byte)_version);
            fs.WriteByte(Newline);
            await JsonSerializer.SerializeAsync(fs, this);
        }

        private HashSet<string> GetListFromKind(EntryKind kind)
            => kind switch
               {
                   EntryKind.File          => FilesToRemove,
                   EntryKind.Directory     => DirectoriesToRemove,
                   EntryKind.RegistryValue => RegistryValuesToRemove,
                   EntryKind.RegistryKey   => RegistryKeysToRemove,
                   _                       => throw new ArgumentOutOfRangeException(nameof(kind), kind.ToString())
               };

        private static async Task<UninstallOperationsList> ReadFromJson(string filePath)
        {
            await using var fs = File.OpenRead(filePath);
            var version = (OperationsListVersion) (byte) fs.ReadByte();
            _ = fs.ReadByte(); // Discard newline
            
            using var json = await JsonDocument.ParseAsync(fs);
            return version switch
                   {
                       OperationsListVersion.v1 => ReadListV1(json, filePath),
                       _ => throw new InvalidOperationException($"Attempted to read Uninstall file at {filePath} which had version {version}")
                   };
        }

        private static UninstallOperationsList ReadListV1(JsonDocument json, string filePath)
            => new(filePath, OperationsListVersion.v1)
            {
                FilesToRemove = ReadAsStringArray(json.RootElement.GetProperty(nameof(FilesToRemove))),
                DirectoriesToRemove = ReadAsStringArray(json.RootElement.GetProperty(nameof(DirectoriesToRemove))),
                RegistryValuesToRemove = ReadAsStringArray(json.RootElement.GetProperty(nameof(RegistryValuesToRemove))),
                RegistryKeysToRemove = ReadAsStringArray(json.RootElement.GetProperty(nameof(RegistryKeysToRemove)))
            };

        private static HashSet<string> ReadAsStringArray(JsonElement element)
        {
            var readElements = new HashSet<string>(element.GetArrayLength());
            using var jsonArray = element.EnumerateArray();
            foreach (var jsonElement in jsonArray)
                if (jsonElement.GetString() is { Length: > 0 } s)
                    readElements.Add(s);

            return readElements;
        }

        public enum EntryKind
        {
            File,
            Directory,
            RegistryKey,
            RegistryValue
        }

        private enum OperationsListVersion: byte
        {
            v1
        }
    }
}