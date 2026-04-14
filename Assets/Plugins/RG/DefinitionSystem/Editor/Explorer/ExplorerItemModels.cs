using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    internal class FolderItem
    {
        public FolderItem(string path, string displayName, bool hasChildren)
        {
            Path = path;
            DisplayName = displayName;
            HasChildren = hasChildren;
        }

        public string Path { get; }
        public string DisplayName { get; }
        public bool HasChildren { get; }
    }

    internal class DefItem
    {
        public DefItem(string path, string displayName, bool isFolder, string typeName, Texture icon)
        {
            Path = path;
            DisplayName = displayName;
            IsFolder = isFolder;
            TypeName = typeName;
            Icon = icon;
        }

        public string Path { get; }
        public string DisplayName { get; }
        public bool IsFolder { get; }
        public string TypeName { get; }
        public Texture Icon { get; }
    }
}
