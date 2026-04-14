using System;
using System.Collections.Generic;
using System.Linq;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace RG.DefinitionSystem.Editor.Drawers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Blueprint), true)]
    public sealed class BlueprintObjectDrawer : BaseScriptableDefinitionDrawer
    {
        private List<(string Path, string Value)> cachedTypes;
        private GenericSelector<string> typeSelector;
        private GUIStyle tIconStyle;
        private Color tIconColor = ColorUtility.TryParseHtmlString("#7fd6fc", out var color) ? color : Color.white;
        
        private PropertyField typeField;
        private OdinImGuiElement typeSelectorElement;

        protected override void Draw(BaseScriptableSourceDefinition scriptableDefinition)
        {
            var blueprint = (Blueprint) scriptableDefinition;
            if (scriptableDefinition.DefinitionType == null)
            {
                EditorGUILayout.HelpBox("Select a type", MessageType.Warning);
                DrawTypeSelection(blueprint);
                return;
            }

            DrawTypeSelection(blueprint);
            base.Draw(scriptableDefinition);
        }
        
        private void DrawTypeSelection(Blueprint blueprint)
        {
            cachedTypes ??= GetTypes();
            typeSelector ??= CreateTypeSelector(blueprint);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.objectField))
            {
                tIconStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, 
                    normal = { textColor = tIconColor },
                    hover = { textColor = tIconColor },
                    active = { textColor = tIconColor }
                };
                GUILayout.Label("T", tIconStyle, GUILayout.Width(10));
                
                if (GUILayout.Button(blueprint.Type, EditorStyles.label))
                {
                    typeSelector.ShowInPopup();
                }
                SirenixEditorGUI.IconButton(EditorIcons.Pen);
            }

            // if (typeSelectorElement == null)
            // {
            //     var root = new VisualElement();
            //     var typeField = new PropertyField();
            //     root.Add(typeField);
            //     // typeField.BindProperty(serializedObject.FindProperty("Type"));
            //     typeSelectorElement = new OdinImGuiElement(root);
            // }
        }
        
        private GenericSelector<string> CreateTypeSelector(Blueprint blueprint)
        {
            var selector = new GenericSelector<string>(cachedTypes.Select(t => t.Path).ToArray());
            selector.SelectionConfirmed += selections =>
            {
                var selectedType = cachedTypes.FirstOrDefault(t => t.Value == blueprint.Type);
                var oldIndex = cachedTypes.IndexOf(selectedType);
                var selection = selections.FirstOrDefault();
                var index = cachedTypes.FindIndex(t => t.Path == selection);
                if (index == oldIndex)
                {
                    return;
                }
                blueprint.Entry = null;
                blueprint.Type = cachedTypes[index].Value;
                blueprint.Entry = Activator.CreateInstance(blueprint.DefinitionType) as Definition;
            };
            return selector;
        }

        private List<(string, string)> GetTypes()
        {
            var types = TypeCache.GetTypesDerivedFrom<Definition>()
                .Where(t => !t.IsAbstract && !t.IsInterface);
            var dictionary = new List<(string, string)>();
            foreach (var type in types)
            {
                var path = GetTypePath(type);
                var title = string.Join("/", path.Select(t => t.Name));
                dictionary.Add((title, type.FullName));
            }

            return dictionary.OrderBy(e => e.Item1).ToList();
        }
        
        private List<Type> GetTypePath(Type type)
        {
            var path = new List<Type> { type };
            while (type != null && type.BaseType != typeof(Definition))
            {
                type = type.BaseType;
                path.Add(type);
            }

            path.Reverse();
            return path;
        }
    }
}