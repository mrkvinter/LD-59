using System.Collections.Generic;
using System.IO;
using Game.Scripts.SaveLoadSystem;
using UnityEngine;

namespace Code.Cheats
{
    public class CurrentSaveCheatCategory : BaseEditorCheatCategory
    {
        private Vector2 scrollPosition;
        private readonly HashSet<string> expandedFolders = new HashSet<string>();

        public override void Draw()
        {
            var debugService = Resolve<DebugSaveLoadService>();
            if (debugService == null)
            {
                GUILayout.Label("DebugSaveLoadService not found");
                return;
            }

#if UNITY_EDITOR
            GUILayout.Label("Saves", UnityEditor.EditorStyles.boldLabel);
#endif

            debugService.StartWithEmptySavePref.Value = GUILayout.Toggle(debugService.StartWithEmptySavePref.Value, "Start with empty save");
            GUILayout.Space(10);
            if (debugService.StartWithEmptySavePref.Value)
            {
                debugService.CurrentSaveNamePref.Value = null;
            }

            var allSaves = debugService.GetAllSaves();
            if (allSaves == null || allSaves.Count == 0)
            {
                GUILayout.Label("No saves found");
                return;
            }

            var tree = BuildTree(allSaves, debugService.SavePath);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)); 

            DrawNode(tree, debugService, 0);
            
            GUILayout.EndScrollView();
        }

        private TreeNode BuildTree(List<string> saves, string rootPath)
        {
            var root = new TreeNode { Name = "Save", FullPath = rootPath };

            foreach (var save in saves)
            {
                var relativePath = save.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                var current = root;
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i == parts.Length - 1)
                    {
                        current.Files.Add(new FileEntry { Name = Path.GetFileNameWithoutExtension(parts[i]), FullPath = save });
                    }
                    else
                    {
                        if (!current.Children.TryGetValue(parts[i], out var child))
                        {
                            child = new TreeNode { Name = parts[i], FullPath = Path.Combine(current.FullPath, parts[i]) };
                            current.Children[parts[i]] = child;
                        }
                        current = child;
                    }
                }
            }

            return root;
        }

        private void DrawNode(TreeNode node, DebugSaveLoadService debugService, int indent)
        {
            foreach (var child in node.Children)
            {
                var folderKey = child.Value.FullPath;
                var isExpanded = expandedFolders.Contains(folderKey);

                GUILayout.BeginHorizontal();
                GUILayout.Space(indent * 20);
#if UNITY_EDITOR
                var newExpanded = UnityEditor.EditorGUILayout.Foldout(isExpanded, child.Key, true);
#else
                var arrow = isExpanded ? "▼" : "►";
                var newExpanded = GUILayout.Toggle(isExpanded, $"{arrow} {child.Key}");
#endif
                if (newExpanded != isExpanded)
                {
                    if (newExpanded)
                        expandedFolders.Add(folderKey);
                    else
                        expandedFolders.Remove(folderKey);
                }
                GUILayout.EndHorizontal();

                if (isExpanded)
                {
                    DrawNode(child.Value, debugService, indent + 1);
                }
            }

            foreach (var file in node.Files)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indent * 20);

                var wasSelected = debugService.CurrentSaveNamePref.Value == file.FullPath;
                var selected = GUILayout.Toggle(file.FullPath == debugService.CurrentSaveNamePref.Value, file.Name);
                if (selected)
                {
                    debugService.CurrentSaveNamePref.Value = file.FullPath;
                    debugService.StartWithEmptySavePref.Value = false;
                }

                if (wasSelected && !selected)
                {
                    debugService.CurrentSaveNamePref.Value = null;
                }

                GUILayout.EndHorizontal();
            }
        }

        private class TreeNode
        {
            public string Name;
            public string FullPath;
            public readonly Dictionary<string, TreeNode> Children = new();
            public readonly List<FileEntry> Files = new();
        }

        private class FileEntry
        {
            public string Name;
            public string FullPath;
        }
    }
}
