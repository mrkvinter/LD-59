using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RG.DefinitionSystem.UnityAdapter;
using UnityEngine.UIElements;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Управляет TreeView дерева папок: построение, раскрытие, выделение, make/bind элементов.
    /// </summary>
    internal class FolderTreeController
    {
        private readonly ExplorerDataService dataService;
        private readonly HashSet<string> expandedFolders = new();
        private readonly Dictionary<int, string> folderIdToPath = new();

        private TreeView treeView;
        private bool isUpdatingSelection;
        private int selectedTreeId = -1;

        public event Action<string> FolderSelected;
        public event Func<string, string, bool> FolderRenamed;
        public event Action<string> FolderDeleted;
        public event Action<string, string> ItemMoved;

        public FolderTreeController(ExplorerDataService dataService)
        {
            this.dataService = dataService;
        }

        public void Bind(TreeView tree)
        {
            treeView = tree;
            treeView.selectionType = SelectionType.Single;
            treeView.makeItem = MakeTreeItem;
            treeView.bindItem = BindTreeItem;
            treeView.selectionChanged += OnSelectionChanged;
            treeView.style.flexGrow = 0f;
            treeView.style.flexShrink = 0;
            
            treeView.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                treeView.style.flexBasis = new StyleLength(StyleKeyword.Auto);
                treeView.style.flexGrow = 0f;
                treeView.style.flexShrink = 0;
            });
        }

        public void InitExpandedRoot()
        {
            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
            expandedFolders.Add(root);
        }

        public void Rebuild(string selectedFolderPath)
        {
            if (treeView == null) return;

            folderIdToPath.Clear();
            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;

            var rootItem = BuildFolderItem(root);
            treeView.SetRootItems(new List<TreeViewItemData<FolderItem>> { rootItem });
            treeView.Rebuild();
            ApplyExpandedState();
            SelectFolderInTree(selectedFolderPath);
        }

        public void ExpandToPath(string folderPath)
        {
            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(root))
            {
                return;
            }

            var current = folderPath;
            while (!string.IsNullOrEmpty(current) &&
                   current.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                expandedFolders.Add(current);
                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent.Equals(current, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                current = parent;
            }

            expandedFolders.Add(root);
        }

        public void SelectFolderInTree(string path)
        {
            if (treeView == null || string.IsNullOrEmpty(path)) return;

            int id = GetFolderId(path);
            if (!folderIdToPath.ContainsKey(id)) return;

            selectedTreeId = id;
            isUpdatingSelection = true;
            treeView.SetSelection(new List<int> { id });
            isUpdatingSelection = false;
        }

        public void ApplyExpandedState()
        {
            if (treeView == null) return;

            foreach (var pair in folderIdToPath)
            {
                if (expandedFolders.Contains(pair.Value))
                {
                    treeView.ExpandItem(pair.Key);
                }
            }
        }

        private TreeViewItemData<FolderItem> BuildFolderItem(string path)
        {
            var subfolders = dataService.CachedDirectories
                .Where(d => Path.GetDirectoryName(d) == path)
                .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var children = new List<TreeViewItemData<FolderItem>>();
            foreach (var subfolder in subfolders)
            {
                children.Add(BuildFolderItem(subfolder));
            }

            bool hasFiles = dataService.CachedFiles.Any(f => Path.GetDirectoryName(f) == path);
            bool hasChildren = children.Count > 0 || hasFiles;

            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
            var displayName = path == root ? "Definitions" : Path.GetFileName(path);
            var data = new FolderItem(path, displayName, hasChildren);
            int id = GetFolderId(path);
            folderIdToPath[id] = path;

            return new TreeViewItemData<FolderItem>(id, data, children);
        }

        private VisualElement MakeTreeItem()
        {
            var renamable = new RenamableListItem();
            renamable.Renamed += (path, newName) => FolderRenamed?.Invoke(path, newName) ?? false;
            renamable.DeleteRequested += (path) => FolderDeleted?.Invoke(path);
            renamable.ItemDropped += (source, target) => ItemMoved?.Invoke(source, target);
            return renamable;
        }

        private void BindTreeItem(VisualElement element, int index)
        {
            var item = treeView.GetItemDataForIndex<FolderItem>(index);
            var id = treeView.GetIdForIndex(index);
            bool isExpanded = treeView.IsExpanded(id);

            var renamable = (RenamableListItem)element;
            renamable.Bind(item.Path, item.DisplayName, ExplorerIconHelper.GetFolderIcon(item.HasChildren, isExpanded), true);
            renamable.SetSelected(id == selectedTreeId);
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            if (isUpdatingSelection) return;

            var prevId = selectedTreeId;
            selectedTreeId = treeView.selectedItems != null ? GetSelectedId() : -1;

            RefreshTreeItemById(prevId);
            RefreshTreeItemById(selectedTreeId);

            foreach (var item in selection)
            {
                if (item is FolderItem folder)
                {
                    FolderSelected?.Invoke(folder.Path);
                }
                break;
            }
        }

        private int GetSelectedId()
        {
            var idx = treeView.selectedIndex;
            return idx >= 0 ? treeView.GetIdForIndex(idx) : -1;
        }

        private void RefreshTreeItemById(int id)
        {
            if (id < 0 || treeView == null) return;
            var index = treeView.viewController.GetIndexForId(id);
            if (index >= 0)
            {
                treeView.RefreshItem(index);
            }
        }

        private static int GetFolderId(string path)
        {
            return StableHash(path.ToLowerInvariant());
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in value)
                {
                    hash = hash * 31 + c;
                }

                return hash;
            }
        }
    }
}
