using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Кеширует иконки редактора, чтобы не вызывать EditorGUIUtility.IconContent при каждом bind.
    /// </summary>
    internal static class ExplorerIconHelper
    {
        private static readonly Dictionary<string, Texture> iconCache = new();
        private static Texture2D jsceIcon;

        public static Texture GetIcon(string iconName)
        {
            if (iconCache.TryGetValue(iconName, out var cached))
                return cached;

            var texture = EditorGUIUtility.IconContent(iconName).image;
            iconCache[iconName] = texture;
            return texture;
        }

        public static Texture GetJsceIcon()
        {
            if (jsceIcon != null)
                return jsceIcon;

            var guids = AssetDatabase.FindAssets("icon t:Texture2D",
                new[] { "Assets/Plugins/RG/DefinitionSystem/Editor/Explorer" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path) == "icon.png")
                {
                    jsceIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    break;
                }
            }

            return jsceIcon ?? GetIcon("TextAsset Icon");
        }

        public static Texture GetFolderIcon(bool hasChildren, bool isExpanded)
        {
            string iconName;
            if (!hasChildren)
                iconName = "FolderEmpty Icon";
            else if (isExpanded)
                iconName = "FolderOpened Icon";
            else
                iconName = "Folder Icon";

            return GetIcon(iconName);
        }
    }
}
