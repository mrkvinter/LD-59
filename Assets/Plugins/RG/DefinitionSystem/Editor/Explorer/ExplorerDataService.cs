using System;
using System.Collections.Generic;
using System.Linq;
using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Сервис данных: кеширование файлов/директорий, загрузка ассетов, список типов для фильтра.
    /// </summary>
    internal class ExplorerDataService
    {
        private readonly Dictionary<string, JsonDefVirtualAsset> assetCache = new();

        public string[] CachedFiles { get; private set; } = Array.Empty<string>();
        public string[] CachedDirectories { get; private set; } = Array.Empty<string>();
        public List<string> TypeFilterChoices { get; } = new() { "All" };

        public void Refresh()
        {
            assetCache.Clear();
            CachedFiles = JsonDefinitionConfigManager.Instance.GetDefFiles();
            CachedDirectories = JsonDefinitionConfigManager.Instance.GetDirectories();
            RefreshTypeFilterChoices();
        }

        public JsonDefVirtualAsset GetOrLoadAsset(string filePath)
        {
            if (assetCache.TryGetValue(filePath, out var asset) && asset != null)
            {
                return asset;
            }

            var (type, entry) = JsonDefinitionConfigManager.Instance.LoadFile(filePath);
            if (string.IsNullOrEmpty(type)) return null;

            asset = ScriptableObject.CreateInstance<JsonDefVirtualAsset>();
            EditorGUIUtility.SetIconForObject(asset, ExplorerIconHelper.GetJsceIcon() as Texture2D);
            asset.Initialize(filePath, type, entry);
            asset.hideFlags = HideFlags.DontSave;

            assetCache[filePath] = asset;
            return asset;
        }

        private void RefreshTypeFilterChoices()
        {
            TypeFilterChoices.Clear();
            TypeFilterChoices.Add("All");

            var types = JsonDefinitionConfigManager.Instance.GetAllDefinitionEntryTypes();
            foreach (var type in types.OrderBy(t => t.Name))
            {
                TypeFilterChoices.Add(type.Name);
            }
        }
    }
}
