using System;
using System.IO;
using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace RG.DefinitionSystem.Editor
{
    public class JsonDefinitionExplorerWindow : EditorWindow
    {
        private const string LayoutUxmlPath =
            "Assets/Plugins/RG/DefinitionSystem/Editor/Explorer/JsonDefinitionExplorerWindow.uxml";

        private ExplorerDataService dataService;
        private FolderTreeController folderTreeController;
        private DefListController defListController;

        private const string SplitViewDimensionPrefKey = "JsonDefinitionExplorer_SplitViewDimension";

        private TwoPaneSplitView twoPaneSplitView;
        private DropdownField typeFilter;
        private bool uiInitialized;

        [SerializeField] private VisualTreeAsset layoutAsset;
        [SerializeField] private string selectedFolderPath;
        [SerializeField] private string selectedFilePath;

        [MenuItem("Window/Definitions")]
        private static void OpenWindow()
        {
            var window = GetWindow<JsonDefinitionExplorerWindow>();
            window.titleContent = new GUIContent("Definitions");
            window.Show();
        }


        private void SaveSplitViewState()
        {
            if (twoPaneSplitView?.fixedPane == null) return;

            float dimension = twoPaneSplitView.fixedPane.resolvedStyle.width;
            if (dimension > 0f)
            {
                EditorPrefs.SetFloat(SplitViewDimensionPrefKey, dimension);
            }
        }

        private void OnEnable()
        {
            dataService = new ExplorerDataService();
            folderTreeController = new FolderTreeController(dataService);
            defListController = new DefListController(dataService);

            folderTreeController.FolderSelected += OnTreeFolderSelected;
            folderTreeController.FolderRenamed += OnItemRenamed;
            folderTreeController.FolderDeleted += OnItemDeleted;
            folderTreeController.ItemMoved += OnItemMoved;
            defListController.FileSelected += OnDefFileSelected;
            defListController.FolderChosen += OnDefFolderChosen;
            defListController.SelectionCleared += OnDefSelectionCleared;
            defListController.ItemRenamed += OnItemRenamed;
            defListController.ItemDeleted += OnItemDeleted;
            defListController.ItemMoved += OnItemMoved;

            JsonDefinitionConfigManager.Instance.Changed += OnExternalDefinitionsChanged;

            dataService.Refresh();

            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
            if (string.IsNullOrEmpty(selectedFolderPath) || !Directory.Exists(selectedFolderPath))
            {
                selectedFolderPath = root;
            }

            folderTreeController.InitExpandedRoot();
        }

        private void OnDisable()
        {
            JsonDefinitionConfigManager.Instance.Changed -= OnExternalDefinitionsChanged;
        }

        private void OnLostFocus()
        {
            //defListController?.ClearSelection();
        }

        private void OnExternalDefinitionsChanged()
        {
            RefreshData();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();

            var visualTree = layoutAsset != null
                ? layoutAsset
                : AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LayoutUxmlPath);

            if (visualTree == null)
            {
                Debug.LogError($"Не удалось загрузить UXML: {LayoutUxmlPath}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            
            CreateToolbar();

            // Type filter
            typeFilter = rootVisualElement.Q<DropdownField>("typeFilter");
            if (typeFilter != null)
            {
                typeFilter.choices = dataService.TypeFilterChoices;
                typeFilter.value = defListController.SelectedTypeFilter;
                typeFilter.RegisterValueChangedCallback(evt =>
                {
                    defListController.SelectedTypeFilter = evt.newValue ?? "All";
                    RebuildContents();
                });
            }

            // Folder tree
            var folderTree = rootVisualElement.Q<TreeView>("folderTree");
            if (folderTree != null)
            {
                folderTreeController.Bind(folderTree);
            }

            // Definition list
            var defList = rootVisualElement.Q<ListView>("contentList");
            if (defList != null)
            {
                defListController.Bind(defList);
            }
            
            twoPaneSplitView = rootVisualElement.Q<TwoPaneSplitView>("splitView");
            if (twoPaneSplitView != null)
            {
                float savedDimension = EditorPrefs.GetFloat(SplitViewDimensionPrefKey, -1f);
                if (savedDimension > 0f)
                {
                    twoPaneSplitView.fixedPaneInitialDimension = savedDimension;
                }

                twoPaneSplitView.RegisterCallback<GeometryChangedEvent>(_ => SaveSplitViewState());
            }

            rootVisualElement.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Show in Explorer", _ => OnShowInExplorerRequested());
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("New Entry", _ => OnContextMenuNewEntry());
                evt.menu.AppendAction("New Folder", _ => OnContextMenuNewFolder());
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", _ => OnContextMenuDelete());
                evt.menu.AppendAction("Rename", _ => OnContextMenuRename());
                evt.menu.AppendAction("Duplicate", _ => OnContextMenuDuplicate());
            }));

            uiInitialized = true;
            RefreshData();
        }

        private void CreateToolbar()
        {
            var contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("New"), false, () => TypeSelectorDialog.Show(GetCurrentFolderPath(), OnEntryCreated));
            contextMenu.AddItem(new GUIContent("New Folder"), false, CreateNewFolder);

            var newButton = rootVisualElement.Q<ToolbarButton>("newButton");
            if (newButton != null)
            {
                newButton.clicked += () => contextMenu.ShowAsContext();
            }
            
            // Search
            var searchField = rootVisualElement.Q<ToolbarSearchField>("searchField");
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(evt =>
                {
                    defListController.SearchQuery = evt.newValue ?? string.Empty;
                    RebuildContents();
                });
            }
        }

        public void RefreshData()
        {
            dataService.Refresh();

            if (typeFilter != null)
            {
                typeFilter.choices = dataService.TypeFilterChoices;
            }

            RebuildTree();
            RebuildContents();
            Repaint();
        }

        public void SelectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Directory.Exists(path))
            {
                folderTreeController.ExpandToPath(path);
                RebuildTree();
                SelectFolder(path, true);
                Repaint();
                return;
            }

            if (File.Exists(path))
            {
                var folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder))
                {
                    folderTreeController.ExpandToPath(folder);
                    RebuildTree();
                    SelectFolder(folder, true);
                }

                SelectFile(path, true);
                Repaint();
            }
        }

        private void OnTreeFolderSelected(string folderPath)
        {
            SelectFolder(folderPath, false);
        }

        private void OnDefFileSelected(string filePath)
        {
            SelectFile(filePath, true);
        }

        private void OnDefFolderChosen(string folderPath)
        {
            SelectFolder(folderPath, true);
        }

        private void OnDefSelectionCleared()
        {
            selectedFilePath = null;
            Selection.activeObject = null;
        }

        private void OnContextMenuNewEntry()
        {
            TypeSelectorDialog.Show(GetCurrentFolderPath(), OnEntryCreated);
        }

        private void OnContextMenuNewFolder()
        {
            CreateNewFolder();
        }

        private void OnContextMenuDelete()
        {
            var pathToDelete = selectedFilePath ?? selectedFolderPath;
            if (string.IsNullOrEmpty(pathToDelete)) return;

            string itemName = Path.GetFileName(pathToDelete);
            if (EditorUtility.DisplayDialog("Delete Item", $"Are you sure you want to delete '{itemName}'?", "Delete", "Cancel"))
            {
                OnItemDeleted(pathToDelete);
            }
        }

        private void OnContextMenuRename()
        {
            var pathToRename = selectedFilePath ?? selectedFolderPath;
            if (string.IsNullOrEmpty(pathToRename)) return;

            defListController.SelectAndRenameItem(pathToRename);
        }

        private void OnContextMenuDuplicate()
        {
            var pathToDuplicate = selectedFilePath ?? selectedFolderPath;
            if (string.IsNullOrEmpty(pathToDuplicate)) return;

            if (File.Exists(pathToDuplicate))
            {
                var directory = Path.GetDirectoryName(pathToDuplicate);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathToDuplicate);
                var extension = Path.GetExtension(pathToDuplicate);
                var newName = GetUniqueName(directory, fileNameWithoutExtension + " Copy", isFolder: false);
                var newPath = Path.Combine(directory, newName + extension);

                File.Copy(pathToDuplicate, newPath);
                RefreshData();
                SelectFile(newPath, false);
            }
            else if (Directory.Exists(pathToDuplicate))
            {
                var parentDirectory = Path.GetDirectoryName(pathToDuplicate);
                var folderName = Path.GetFileName(pathToDuplicate);
                var newName = GetUniqueName(parentDirectory, folderName + " Copy", isFolder: true);
                var newPath = Path.Combine(parentDirectory, newName);

                CopyDirectory(pathToDuplicate, newPath);
                RefreshData();
                SelectFolder(newPath, true);
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(subDir);
                var destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(subDir, destSubDir);
            }
        }

        private void CreateNewFolder()
        {
            var parentPath = GetCurrentFolderPath();
            var folderName = GetUniqueName(parentPath, "New Folder", isFolder: true);
            var fullPath = Path.Combine(parentPath, folderName);
            Directory.CreateDirectory(fullPath);
            RefreshData();
            defListController.SelectAndRenameItem(fullPath);
        }

        private void OnEntryCreated(string createdFilePath)
        {
            RefreshData();
            SelectFile(createdFilePath, false);
            defListController.SelectAndRenameItem(createdFilePath);
        }

        private static string GetUniqueName(string parentPath, string baseName, bool isFolder)
        {
            var candidate = baseName;
            var number = 1;
            while (true)
            {
                var fullPath = Path.Combine(parentPath, isFolder ? candidate : candidate + ".def");
                if (isFolder ? !Directory.Exists(fullPath) : !File.Exists(fullPath))
                    return candidate;
                candidate = $"{baseName} {number}";
                number++;
            }
        }

        private void OnShowInExplorerRequested()
        {
            var selectedPath = selectedFilePath ?? selectedFolderPath;
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            EditorUtility.RevealInFinder(selectedPath);
        }

        private bool OnItemRenamed(string oldPath, string newName)
        {
            var directory = Path.GetDirectoryName(oldPath);
            if (string.IsNullOrEmpty(directory)) return false;

            if (File.Exists(newName) || Directory.Exists(newName)) return false;

            string newPath;
            if (File.Exists(oldPath))
            {
                var extension = Path.GetExtension(oldPath);
                newPath = Path.Combine(directory, newName + extension);
            }
            else
            {
                newPath = Path.Combine(directory, newName);
            }

            if (oldPath == newPath) return false;

            JsonDefinitionConfigManager.Instance.Rename(oldPath, newPath);

            if (selectedFilePath == oldPath)
            {
                selectedFilePath = newPath;
            }

            if (selectedFolderPath == oldPath)
            {
                selectedFolderPath = newPath;
            }

            RefreshData();
            return true;
        }

        private void OnItemMoved(string sourcePath, string targetFolderPath)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(targetFolderPath)) return;

            var fileName = Path.GetFileName(sourcePath);
            var newPath = Path.Combine(targetFolderPath, fileName);

            if (sourcePath == newPath) return;

            if (File.Exists(newPath) || Directory.Exists(newPath))
            {
                Debug.LogWarning($"[JSCE] Target already exists: {newPath}");
                return;
            }

            JsonDefinitionConfigManager.Instance.Rename(sourcePath, newPath);

            if (selectedFilePath == sourcePath)
            {
                selectedFilePath = newPath;
            }

            if (selectedFolderPath != null && selectedFolderPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase))
            {
                selectedFolderPath = newPath + selectedFolderPath.Substring(sourcePath.Length);
            }

            RefreshData();
        }

        private void OnItemDeleted(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (File.Exists(path))
            {
                JsonDefinitionConfigManager.Instance.DeleteFile(path);
                if (selectedFilePath == path)
                {
                    selectedFilePath = null;
                    Selection.activeObject = null;
                }
            }
            else if (Directory.Exists(path))
            {
                if (path == JsonDefinitionConfigManager.Instance.ExternalFolderPath)
                {
                    Debug.LogWarning("[JSCE] Cannot delete root folder.");
                    return;
                }

                JsonDefinitionConfigManager.Instance.DeleteDirectory(path);

                if (selectedFolderPath != null && selectedFolderPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    selectedFolderPath = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
                    selectedFilePath = null;
                    Selection.activeObject = null;
                }
            }

            RefreshData();
        }

        private void SelectFolder(string path, bool updateTree)
        {
            selectedFolderPath = path;
            selectedFilePath = null;
            Selection.activeObject = null;
            RebuildContents();
            if (updateTree)
            {
                folderTreeController.ExpandToPath(path);
                folderTreeController.ApplyExpandedState();
                folderTreeController.SelectFolderInTree(path);
            }
        }

        private void SelectFile(string filePath, bool updateTree)
        {
            selectedFilePath = filePath;
            var asset = dataService.GetOrLoadAsset(filePath);
            Selection.activeObject = asset;
            if (updateTree)
            {
                var folder = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(folder))
                {
                    folderTreeController.ExpandToPath(folder);
                    folderTreeController.ApplyExpandedState();
                    folderTreeController.SelectFolderInTree(folder);
                }
            }

            defListController.SelectFileInList(filePath);
        }

        private string GetCurrentFolderPath()
        {
            var root = JsonDefinitionConfigManager.Instance.ExternalFolderPath;
            if (string.IsNullOrEmpty(selectedFolderPath) || !Directory.Exists(selectedFolderPath))
            {
                return root;
            }

            return selectedFolderPath;
        }

        private void RebuildTree()
        {
            if (!uiInitialized) return;
            folderTreeController.Rebuild(selectedFolderPath);
        }

        private void RebuildContents()
        {
            if (!uiInitialized) return;
            defListController.RebuildContents(GetCurrentFolderPath(), selectedFilePath);
        }
    }
}
