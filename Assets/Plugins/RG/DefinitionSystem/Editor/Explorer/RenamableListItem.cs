using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Переиспользуемый элемент списка/дерева с иконкой, лейблом и возможностью переименования.
    /// Rename запускается при повторном клике по уже выделенному элементу (не double-click).
    /// </summary>
    internal sealed class RenamableListItem : VisualElement
    {
        private const long RenameDelayMs = 300;
        private const long DoubleClickThresholdMs = 500;
        private readonly Image icon;
        private readonly Label label;
        private readonly TextField input;
        private readonly DragAndDropManipulator dragManipulator;

        private string currentPath;
        private bool isRenaming;
        private bool isSelected;
        private bool isFolder;

        private long lastClickTimeMs;
        private IVisualElementScheduledItem renameSchedule;

        public event Func<string, string, bool> Renamed;
        public event Action<string> DeleteRequested;

        /// <summary>
        /// Вызывается при drop файла на эту папку. Аргументы: sourcePath, targetFolderPath.
        /// </summary>
        public event Action<string, string> ItemDropped;

        public RenamableListItem()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.flexGrow = 1f;
            focusable = true;

            icon = new Image { name = "icon" };
            Add(icon);

            label = new Label { name = "label" };
            label.style.flexGrow = 1f;
            Add(label);

            input = new TextField { name = "input" };
            input.style.flexGrow = 1f;
            input.style.display = DisplayStyle.None;
            input.RegisterCallback<KeyDownEvent>(OnInputKeyDown);
            input.RegisterCallback<FocusOutEvent>(OnInputFocusOut);
            Add(input);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            dragManipulator = new DragAndDropManipulator(() => currentPath, () => isFolder);
            dragManipulator.DragStarted += CancelRenameArm;
            dragManipulator.ItemDropped += (source, target) => ItemDropped?.Invoke(source, target);
            this.AddManipulator(dragManipulator);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.F2)
            {
                TryEnterRename();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.Delete)
            {
                ShowDeleteConfirmation();
            }
        }

        private void ShowDeleteConfirmation()
        {
            if (string.IsNullOrEmpty(currentPath)) return;

            string itemName = Path.GetFileName(currentPath);
            if (EditorUtility.DisplayDialog("Delete Item", $"Are you sure you want to delete '{itemName}'?", "Delete", "Cancel"))
            {
                DeleteRequested?.Invoke(currentPath);
            }
        }

        public string CurrentPath => currentPath;
        public bool IsFolder => isFolder;

        public void Bind(string path, string displayName, Texture iconTexture, bool folder = false)
        {
            currentPath = path;
            isFolder = folder;
            icon.image = iconTexture;
            label.text = displayName;
            input.value = displayName;
            CancelRenameArm();
            ExitRenameMode();
        }

        public void SetSelected(bool selected)
        {
            if (!selected && isSelected)
            {
                CancelRenameArm();
                if (isRenaming) ExitRenameMode();
            }

            isSelected = selected;

            if (!selected)
            {
                lastClickTimeMs = 0;
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                CancelRenameArm();
                return;
            }

            if (isRenaming) return;

            var nowMs = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            if (!isSelected)
            {
                lastClickTimeMs = nowMs;
                return;
            }

            var elapsed = nowMs - lastClickTimeMs;
            lastClickTimeMs = nowMs;

            if (elapsed > 0 && elapsed < DoubleClickThresholdMs)
            {
                CancelRenameArm();
                return;
            }

            ArmRename();
        }

        private void ArmRename()
        {
            CancelRenameArm();
            renameSchedule = schedule.Execute(TryEnterRename).StartingIn(RenameDelayMs);
        }

        private void CancelRenameArm()
        {
            if (renameSchedule != null)
            {
                renameSchedule.Pause();
                renameSchedule = null;
            }
        }

        private void TryEnterRename()
        {
            renameSchedule = null;

            if (!isSelected || isRenaming) return;

            EnterRenameMode();
        }

        public void EnterRenameMode()
        {
            isRenaming = true;
            label.style.display = DisplayStyle.None;
            input.style.display = DisplayStyle.Flex;
            input.value = label.text;

            input.schedule.Execute(() =>
            {
                input.Focus();
                input.SelectAll();
            });
        }

        private void ExitRenameMode()
        {
            isRenaming = false;
            label.style.display = DisplayStyle.Flex;
            input.style.display = DisplayStyle.None;
        }

        private void TryCommitRename()
        {
            var newName = input.value?.Trim();
            var oldName = label.text;

            ExitRenameMode();

            if (string.IsNullOrEmpty(newName) || newName == oldName)
            {
                return;
            }
            
            var newPath = Path.Combine(Directory.GetParent(CurrentPath)!.FullName, newName);

            if (Renamed?.Invoke(CurrentPath, newPath) == true)
                label.text = newName;
        }

        private void OnInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                TryCommitRename();
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                ExitRenameMode();
                evt.StopPropagation();
            }
        }

        private void OnInputFocusOut(FocusOutEvent evt)
        {
            if (isRenaming)
            {
                TryCommitRename();
            }
        }
    }
}
