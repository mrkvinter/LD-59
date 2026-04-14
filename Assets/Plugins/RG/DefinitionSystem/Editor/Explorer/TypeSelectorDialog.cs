using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Вспомогательный диалог для выбора типа при создании нового .def файла.
    /// </summary>
    public class TypeSelectorDialog : OdinEditorWindow
    {

        [ValueDropdown("GetAllTypes")]
        [LabelText("Select Definition Type")]
        public Type SelectedType;

        private string targetPath;
        private Action<string> onCreated;
        
        public static void Show(string targetPath, Action<string> onCreated = null)
        {
            var window = GetWindow<TypeSelectorDialog>(true, "Create New JSCE Entry", true);
            window.targetPath = targetPath;
            window.onCreated = onCreated;
            window.ShowUtility();
        }

        private IEnumerable<Type> GetAllTypes()
        {
            return JsonDefinitionConfigManager.Instance.GetAllDefinitionEntryTypes();
        }

        [Button("Create", ButtonSizes.Large)]
        private void Create()
        {
            if (SelectedType == null) return;

            var baseName = "New " + SelectedType.Name;
            var fileName = GetUniqueFileName(targetPath, baseName);
            string fullPath = Path.Combine(targetPath, fileName + ".def");

            var entry = (Definition)Activator.CreateInstance(SelectedType);

            JsonDefinitionConfigManager.Instance.SaveFile(fullPath, SelectedType.FullName, entry);

            if (onCreated != null)
            {
                onCreated.Invoke(fullPath);
            }
            else
            {
                var manager = GetWindow<JsonDefinitionExplorerWindow>();
                manager.RefreshData();
            }

            Close();
        }

        private static string GetUniqueFileName(string parentPath, string baseName)
        {
            var candidate = baseName;
            var number = 1;
            while (File.Exists(Path.Combine(parentPath, candidate + ".def")))
            {
                candidate = $"{baseName} {number}";
                number++;
            }
            return candidate;
        }
    }
}
