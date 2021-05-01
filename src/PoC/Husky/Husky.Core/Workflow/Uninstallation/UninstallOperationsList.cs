using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace Husky.Core.Workflow.Uninstallation
{
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

        public string FilePath { get; }

        private readonly OperationsListVersion _version;
        private static readonly ILogger Logger = Log.ForContext(Constants.SourceContextPropertyName, nameof(UninstallOperationsList));

        private const byte Newline = 0xA;

        private UninstallOperationsList(string filePath, OperationsListVersion version)
        {
            FilePath = filePath;
            _version = version;
        }

        public static async ValueTask<IUninstallOperationsList> CreateOrRead(string filePath)
        {
            Logger.Information("Initializing instance of Uninstall Operations List at directory {uninstallOperationsFile}", filePath);
            if (File.Exists(filePath))
                return await ReadFromJson(filePath);

            Logger.Debug("Unable to locate existing Uninstall Operations List, creating new copy");
            var newUninstallOpsList = new UninstallOperationsList(filePath, OperationsListVersion.V1);
            var parentDirectory = new FileInfo(filePath).DirectoryName ?? throw new ArgumentException($"Unable to locate directory for uninstall file at {filePath}");

            Directory.CreateDirectory(parentDirectory);
            File.Create(filePath).Close();
            await newUninstallOpsList.Flush();
            
            Logger.Information("Successfully created new instance of Uninstall Operations List at directory {uninstallOperationsFile}", filePath);

            return newUninstallOpsList;
        }

        public void AddEntry(EntryKind kind, string entry)
        {
            Logger.Verbose("Adding {entryKind} entry to Uninstall Operations List with value {entry}", kind, entry);
            GetListFromKind(kind).Add(entry);
        }

        public IEnumerable<string> ReadEntries(EntryKind kind) => GetListFromKind(kind);

        public async Task Flush()
        {
            Logger.Debug("Writing {numFiles} files, {numDirectories} directories, {numRegValues} registry values, and {numRegKeys} keys to Uninstall OperationsList",
                FilesToRemove.Count, DirectoriesToRemove.Count, RegistryValuesToRemove.Count, RegistryKeysToRemove.Count);

            var uninstallOperationsFileExists = File.Exists(FilePath);
            if (!uninstallOperationsFileExists)
                Logger.Warning("Uninstall Operations List was deleted at {uninstallOperationsListFilePath} -- recreating", FilePath);

            await using var fs = File.Open(FilePath, FileMode.Create);

            fs.WriteByte((byte)_version);
            fs.WriteByte(Newline);
            await JsonSerializer.SerializeAsync(fs, this);
            Logger.Debug("Successfully flushed Uninstall Operations List");
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
            Logger.Debug("Reading existing Uninstall Operations file at {filePath}", filePath);
            await using var fs = File.OpenRead(filePath);
            var version = (OperationsListVersion) (byte) fs.ReadByte();
            Logger.Verbose("Reading file as version {uninstallOperationsFileVersion}", version.ToString());
            _ = fs.ReadByte(); // Discard newline
            
            using var json = await JsonDocument.ParseAsync(fs);
            return version switch
                   {
                       OperationsListVersion.V1 => ReadListV1(json, filePath),
                       _ => throw new InvalidOperationException($"Attempted to read Uninstall file at {filePath} which had version {version}")
                   };
        }

        private static UninstallOperationsList ReadListV1(JsonDocument json, string filePath)
            => new(filePath, OperationsListVersion.V1)
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
            V1
        }
    }
}