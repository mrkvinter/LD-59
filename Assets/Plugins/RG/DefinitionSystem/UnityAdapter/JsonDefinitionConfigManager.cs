using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    internal class JsonDefinitionConfigManager
    {
        private const string FolderName = "Definitions";

        private static JsonDefinitionConfigManager instance;
        public static JsonDefinitionConfigManager Instance => instance ??= new JsonDefinitionConfigManager();

        private readonly Dictionary<string, DefinitionEntry> definitionCache = new();
        private readonly JsonSerializerSettings serializerSettings;
        private bool isLoaded;
        
        public event Action Changed;

        public string ExternalFolderPath => 
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, FolderName);

        private JsonDefinitionConfigManager()
        {
            serializerSettings = CreateSerializerSettings();
        }

        private static JsonSerializerSettings CreateSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefinitionContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new UnityObjectJsonConverter()
                }
            };
        }

        public string[] GetDefFiles()
        {
            string path = ExternalFolderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Directory.GetFiles(path, "*.def", SearchOption.AllDirectories);
        }

        public string[] GetDirectories()
        {
            string path = ExternalFolderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        }

        public string GetRelativePath(string fullPath)
        {
            string root = ExternalFolderPath;
            if (fullPath.StartsWith(root))
            {
                return fullPath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            return fullPath;
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                definitionCache.Remove(filePath);
            }
        }

        public void DeleteDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
                
                var keysToRemove = definitionCache.Keys
                    .Where(k => k.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    definitionCache.Remove(key);
                }
            }
        }

        public void Rename(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
                if (definitionCache.Remove(oldPath, out var entry))
                {
                    definitionCache[newPath] = entry;
                }
            }
            else if (Directory.Exists(oldPath))
            {
                // if (!Directory.Exists(newPath))
                //     Directory.CreateDirectory(newPath);

                Directory.Move(oldPath, newPath);
                
                var keysToMove = definitionCache.Keys
                    .Where(k => k.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var key in keysToMove)
                {
                    if (definitionCache.Remove(key, out var entry))
                    {
                        var updatedKey = newPath + key.Substring(oldPath.Length);
                        definitionCache[updatedKey] = entry;
                    }
                }
            }
        }

        public (string type, Definition entry) LoadFile(string filePath)
        {
            try
            {
                string jsonText = File.ReadAllText(filePath);
                var jObj = JObject.Parse(jsonText);

                var typeName = jObj["type"]?.Value<string>();
                if (string.IsNullOrEmpty(typeName))
                    return (null, null);

                Type entryType = GetDefinitionEntryType(typeName);
                if (entryType == null)
                    return (typeName, null);

                jObj.Remove("type");

                var serializer = JsonSerializer.Create(serializerSettings);
                Definition entry = (Definition)jObj.ToObject(entryType, serializer);

                if (entry != null)
                    definitionCache[filePath] = new DefinitionEntry(typeName, filePath, entry);

                return (typeName, entry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Def] Failed to load {filePath}: {ex.Message}");
                return (null, null);
            }
        }

        public void SaveFile(string filePath, string type, Definition entry)
        {
            try
            {
                if (entry == null) return;

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var serializer = JsonSerializer.Create(serializerSettings);
                var jObj = JObject.FromObject(entry, serializer);

                var result = new JObject { { "type", type } };
                foreach (var prop in jObj.Properties())
                    result.Add(prop.Name, prop.Value);

                File.WriteAllText(filePath, result.ToString(Formatting.Indented));

                definitionCache[filePath] = new DefinitionEntry(type, filePath, entry);
                Debug.Log($"[Def] Saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Def] Failed to save {filePath}: {ex.Message}");
            }
        }

        public Type GetDefinitionEntryType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    typeof(Definition).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    (t.FullName == typeName || t.Name == typeName));
        }

        public IEnumerable<Type> GetAllDefinitionEntryTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Definition).IsAssignableFrom(t) && !t.IsAbstract);
        }

        /// <summary>
        /// Загружает все .def файлы и возвращает список Definition-ов.
        /// </summary>
        public IReadOnlyList<Definition> LoadAllDefinitions()
        {
            isLoaded = true;
            definitionCache.Clear();
            var files = GetDefFiles();
            var result = new List<Definition>();

            foreach (var file in files)
            {
                var (type, entry) = LoadFile(file);
                if (entry != null)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        public IReadOnlyList<(string path, TDef def)> GetDefinitions<TDef>() where TDef : Definition
        {
            if (!isLoaded) LoadAllDefinitions();

            return definitionCache.Values
                .Where(e => e.Type == typeof(TDef).FullName)
                .Select(e => (e.Path, (TDef) e.Definition))
                .ToList();
        }

        /// <summary>
        /// Возвращает текущий кэш загруженных Definition-ов.
        /// </summary>
        public IReadOnlyDictionary<string, DefinitionEntry> CachedDefinitions => definitionCache;

        public void RaiseChanged()
        {
            Changed?.Invoke();
        }

        public readonly struct DefinitionEntry
        {
            public readonly string Type;
            public readonly string Path;
            public readonly Definition Definition;

            public DefinitionEntry(string type, string path, Definition definition)
            {
                Type = type;
                Path = path;
                Definition = definition;
            }
        }

        private static void MigrateBackingFields(JObject jObj)
        {
            var renames = new List<(string oldName, string newName)>();
            foreach (var prop in jObj.Properties())
            {
                if (prop.Name.StartsWith("<") && prop.Name.EndsWith(">k__BackingField"))
                {
                    var cleanName = prop.Name.Substring(1, prop.Name.IndexOf('>') - 1);
                    renames.Add((prop.Name, cleanName));
                }
            }

            foreach (var (oldName, newName) in renames)
            {
                var value = jObj[oldName];
                jObj.Remove(oldName);
                jObj[newName] = value;
            }
        }

        private class DefinitionContractResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var members = base.GetSerializableMembers(objectType);

                var privateSerializedFields = objectType
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(f => f.IsDefined(typeof(SerializeField), inherit: true))
                    .Cast<MemberInfo>();

                foreach (var field in privateSerializedFields)
                {
                    if (!members.Contains(field))
                        members.Add(field);
                }

                return members;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (member is FieldInfo fi && fi.IsDefined(typeof(SerializeField), inherit: true))
                {
                    prop.Readable = true;
                    prop.Writable = true;
                }
                else if (!prop.Writable && member is PropertyInfo pi)
                {
                    var setter = pi.GetSetMethod(true);
                    if (setter != null)
                        prop.Writable = true;
                    else
                        prop.Ignored = true;
                }

                return prop;
            }
        }

        private class UnityObjectJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(UnityEngine.Object).IsAssignableFrom(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
#if UNITY_EDITOR
                if (value is UnityEngine.Object unityObj && unityObj != null)
                {
                    if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(unityObj, out string guid, out long localId))
                    {
                        writer.WritePropertyName("guid");
                        writer.WriteValue(guid);
                        writer.WritePropertyName("fileID");
                        writer.WriteValue(localId);
                    }
                }
#endif
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var jObj = JObject.Load(reader);

#if UNITY_EDITOR
                // Новый формат: guid + fileID
                var guid = jObj["guid"]?.Value<string>();
                if (!string.IsNullOrEmpty(guid))
                {
                    var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath))
                        return null;

                    var fileId = jObj["fileID"]?.Value<long>() ?? 0;
                    if (fileId == 0)
                        return UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);

                    var allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var asset in allAssets)
                    {
                        if (asset != null && 
                            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string _, out long id) && 
                            id == fileId)
                        {
                            return asset;
                        }
                    }

                    return UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                }

                // Обратная совместимость: старый формат instanceID
                var instanceId = jObj["instanceID"]?.Value<int>() ?? 0;
                if (instanceId != 0)
                    return UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
#endif
                return null;
            }
        }

#if UNITY_EDITOR
        private FileSystemWatcher watcher;
        private bool isDirty;
        private double lastProcessTime;
        private const double DebounceDelay = 0.3;

        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeWatcher()
        {
            Instance.SetupWatcher();
        }

        private void SetupWatcher()
        {
            var path = ExternalFolderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            watcher = new FileSystemWatcher(path)
            {
                Filter = "*.*",
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Created += OnFileSystemChanged;
            watcher.Deleted += OnFileSystemChanged;
            watcher.Changed += OnFileSystemChanged;
            watcher.Renamed += OnFileSystemChanged;

            UnityEditor.EditorApplication.update += OnEditorUpdate;
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            isDirty = true;
        }

        private void OnEditorUpdate()
        {
            if (!isDirty) return;

            if (UnityEditor.EditorApplication.timeSinceStartup - lastProcessTime < DebounceDelay)
                return;

            isDirty = false;
            lastProcessTime = UnityEditor.EditorApplication.timeSinceStartup;

            LoadAllDefinitions();
            RaiseChanged();
        }
#endif
    }
}
