using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RG.DefinitionSystem.Editor
{
    internal sealed class DragAndDropManipulator : Manipulator
    {
        private const float DragThreshold = 5f;

        private bool isDragCandidate;
        private Vector2 dragStartPos;

        private readonly Func<string> getPath;
        private readonly Func<bool> getIsFolder;

        public event Action DragStarted;
        public event Action<string, string> ItemDropped;

        public DragAndDropManipulator(Func<string> getPath, Func<bool> getIsFolder)
        {
            this.getPath = getPath;
            this.getIsFolder = getIsFolder;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdated);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            var path = getPath();
            if (string.IsNullOrEmpty(path)) return;

            isDragCandidate = true;
            dragStartPos = evt.position;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragCandidate) return;

            var path = getPath();
            if (string.IsNullOrEmpty(path)) return;

            var delta = (Vector2)evt.position - dragStartPos;
            if (delta.magnitude < DragThreshold) return;

            isDragCandidate = false;
            DragStarted?.Invoke();

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("RenamableListItemPath", path);
            DragAndDrop.paths = new[] { path };
            DragAndDrop.StartDrag(Path.GetFileName(path));

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            isDragCandidate = false;
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            var sourcePath = DragAndDrop.GetGenericData("RenamableListItemPath") as string;
            if (string.IsNullOrEmpty(sourcePath)) return;

            if (!getIsFolder())
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
                return;
            }

            var sourceDir = Path.GetDirectoryName(sourcePath);
            var currentPath = getPath();
            if (string.Equals(sourceDir, currentPath, StringComparison.OrdinalIgnoreCase))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }

            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            var sourcePath = DragAndDrop.GetGenericData("RenamableListItemPath") as string;
            if (string.IsNullOrEmpty(sourcePath)) return;

            if (!getIsFolder()) return;

            var sourceDir = Path.GetDirectoryName(sourcePath);
            var currentPath = getPath();
            if (string.Equals(sourceDir, currentPath, StringComparison.OrdinalIgnoreCase))
                return;

            DragAndDrop.AcceptDrag();
            ItemDropped?.Invoke(sourcePath, currentPath);

            evt.StopPropagation();
        }
    }
}
