using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RG.DefinitionSystem.UnityAdapter;
using UnityEngine.UIElements;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Управляет ListView содержимого папки: построение списка, поиск, фильтрация по типу, make/bind элементов.
    /// </summary>
    internal class DefListController
    {
        private readonly ExplorerDataService dataService;
        private readonly List<DefItem> defItems = new();

        private ListView listView;
        private bool isUpdatingSelection;
        private int selectedIndex = -1;

        private string searchQuery = string.Empty;
        private string selectedTypeFilter = "All";

        public event Action<string> FileSelected;
        public event Action<string> FolderChosen;
        public event Action SelectionCleared;
        public event Func<string, string, bool> ItemRenamed;
        public event Action<string> ItemDeleted;
        public event Action<string, string> ItemMoved;

        public string SearchQuery
        {
            get => searchQuery;
            set => searchQuery = value ?? string.Empty;
        }

        public string SelectedTypeFilter
        {
            get => selectedTypeFilter;
            set => selectedTypeFilter = value ?? "All";
        }

        public DefListController(ExplorerDataService dataService)
        {
            this.dataService = dataService;
        }

        public void Bind(ListView list)
        {
            listView = list;
            listView.selectionType = SelectionType.Single;
            listView.makeItem = MakeDefItem;
            listView.bindItem = BindDefItem;
            listView.itemsSource = defItems;
            listView.selectionChanged += OnSelectionChanged;
            listView.itemsChosen += OnItemsChosen;
            listView.style.flexGrow = 1f;

            listView.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                listView.style.flexBasis = new StyleLength(StyleKeyword.Auto);
                listView.style.flexGrow = 1;
                listView.style.flexShrink = 0;
            });

            listView.RegisterCallback<PointerDownEvent>(evt =>
            {
                var scrollView = listView.Q<ScrollView>();
                var contentContainer = scrollView?.contentContainer;
                if (contentContainer == null) return;

                var localPos = contentContainer.WorldToLocal(evt.position);
                if (localPos.y > contentContainer.layout.height || !contentContainer.layout.Contains(localPos))
                {
                    listView.ClearSelection();
                    SelectionCleared?.Invoke();
                }
            });
        }

        public void ClearSelection()
        {
            listView.ClearSelection();
            SelectionCleared?.Invoke();
        }

        public void RebuildContents(string folderPath, string selectedFilePath)
        {
            if (listView == null) return;

            defItems.Clear();

            if (!string.IsNullOrEmpty(folderPath))
            {
                var subfolders = dataService.CachedDirectories
                    .Where(d => Path.GetDirectoryName(d) == folderPath)
                    .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var subfolder in subfolders)
                {
                    var name = Path.GetFileName(subfolder);
                    bool hasChildren = dataService.CachedDirectories
                                           .Any(d => Path.GetDirectoryName(d) == subfolder)
                                       || dataService.CachedFiles
                                           .Any(f => Path.GetDirectoryName(f) == subfolder);
                    defItems.Add(new DefItem(subfolder, name, true, null,
                        ExplorerIconHelper.GetFolderIcon(hasChildren, false)));
                }

                var files = dataService.CachedFiles
                    .Where(f => Path.GetDirectoryName(f) == folderPath)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var file in files)
                {
                    var name = Path.GetFileNameWithoutExtension(file);

                    if (!string.IsNullOrEmpty(searchQuery) &&
                        name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    if (selectedTypeFilter != "All")
                    {
                        var (typeName, _) = JsonDefinitionConfigManager.Instance.LoadFile(file);
                        if (string.IsNullOrEmpty(typeName))
                        {
                            continue;
                        }

                        var entryType = JsonDefinitionConfigManager.Instance.GetDefinitionEntryType(typeName);
                        if (entryType == null || entryType.Name != selectedTypeFilter)
                        {
                            continue;
                        }
                    }

                    defItems.Add(new DefItem(file, name, false, null,
                        ExplorerIconHelper.GetJsceIcon()));
                }
            }

            listView.Rebuild();

            isUpdatingSelection = true;
            selectedIndex = -1;
            listView.ClearSelection();
            isUpdatingSelection = false;

            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                SelectFileInList(selectedFilePath);
            }
        }

        public void SelectFileInList(string filePath)
        {
            if (listView == null || string.IsNullOrEmpty(filePath)) return;

            int index = defItems.FindIndex(item => !item.IsFolder && item.Path == filePath);
            if (index < 0) return;

            selectedIndex = index;
            isUpdatingSelection = true;
            listView.SetSelection(index);
            isUpdatingSelection = false;
        }

        public void SelectAndRenameItem(string path)
        {
            if (listView == null || string.IsNullOrEmpty(path)) return;

            int index = defItems.FindIndex(item => item.Path == path);
            if (index < 0) return;

            selectedIndex = index;
            isUpdatingSelection = true;
            listView.SetSelection(index);
            isUpdatingSelection = false;

            listView.ScrollToItem(index);
            listView.schedule.Execute(() =>
            {
                var element = listView.GetRootElementForIndex(index);
                if (element is RenamableListItem renamable)
                {
                    renamable.SetSelected(true);
                    renamable.EnterRenameMode();
                }
            }).StartingIn(50);
        }

        private VisualElement MakeDefItem()
        {
            var renamable = new RenamableListItem();
            renamable.Renamed += (path, newName) => ItemRenamed?.Invoke(path, newName) ?? false;
            renamable.DeleteRequested += (path) => ItemDeleted?.Invoke(path);
            renamable.ItemDropped += (source, target) => ItemMoved?.Invoke(source, target);
            return renamable;
        }

        private void BindDefItem(VisualElement element, int index)
        {
            var item = defItems[index];
            var renamable = (RenamableListItem)element;
            renamable.Bind(item.Path, item.DisplayName, item.Icon, item.IsFolder);
            renamable.SetSelected(index == selectedIndex);
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            if (isUpdatingSelection) return;

            var prevIndex = selectedIndex;
            selectedIndex = listView.selectedIndex;

            UpdateSelectedVisual(prevIndex);
            UpdateSelectedVisual(selectedIndex);

            foreach (var item in selection)
            {
                if (item is DefItem { IsFolder: false } defItem)
                {
                    FileSelected?.Invoke(defItem.Path);
                }
                break;
            }
        }

        private void UpdateSelectedVisual(int index)
        {
            if (index < 0) return;
            // Refresh the element at index to update SetSelected
            listView.RefreshItem(index);
        }

        private void OnItemsChosen(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                if (item is DefItem { IsFolder: true } defItem)
                {
                    FolderChosen?.Invoke(defItem.Path);
                }
                break;
            }
        }
    }
}
