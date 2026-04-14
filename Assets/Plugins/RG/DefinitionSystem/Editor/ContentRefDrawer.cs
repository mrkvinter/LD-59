using System;
using System.Linq;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.Editor.Drawers;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    public sealed class DefRefDrawer<TDef> : OdinValueDrawer<DefRef<TDef>>
        where TDef : Definition
    {
        private BaseScriptableDefinition selectedDefinition;
        private Type scriptableObjectType;
        private UnityEditor.Editor defEditor;
        private bool showDefEditor;
        private bool subscriptionInitialized;

        private DefSelector<TDef> selector;
        private DefAssetType? assetType;
        

        protected override void DrawPropertyLayout(GUIContent label)
        {
            assetType ??= DefinitionDatabaseUtility.GetAssetType<TDef>();

            switch (assetType)
            {
                case DefAssetType.None:
                    SirenixEditorGUI.ErrorMessageBox("No definition found for type " + typeof(TDef).Name);
                    return;
                case DefAssetType.TableDef:
                    DrawTableDef(label);
                    return;
                case DefAssetType.ScriptableDef:
                    DrawScriptableDef(label);
                    return;
            }
        }

        private void DrawTableDef(GUIContent label)
        {
            if (selector == null)
            {
                selector = new DefSelector<TDef>();
                selector.EnableSingleClickToSelect();
                selector.SetSelection(ValueEntry.SmartValue.Id);
            }

            var result = selector.GetCurrentSelection().FirstOrDefault();
            if (result.Id != ValueEntry.SmartValue.Id)
            {
                selector.SetSelection(ValueEntry.SmartValue.Id);
                result = ValueEntry.SmartValue;
            }

            var controlRect = EditorGUILayout.GetControlRect();

            GUILayout.BeginHorizontal();

            var labelWidth =  label != null && label != GUIContent.none ? GUIHelper.BetterLabelWidth : 0;
            var rect = new Rect(controlRect.x + labelWidth, controlRect.y, controlRect.width - labelWidth,
                controlRect.height);

            label ??= GUIContent.none;
            var value = result;
            var text = string.IsNullOrEmpty(value.Id) ? "None" : value.Id;
            if (EditorGUI.DropdownButton(rect, new GUIContent(text), FocusType.Keyboard))
            {
                if (!subscriptionInitialized)
                {
                    subscriptionInitialized = true;
                    selector.SelectionChanged += selection =>
                    {
                        var selected = selection.FirstOrDefault();
                        ValueEntry.SmartValue = selected;
                        selectedDefinition = GetDefinitionObject(selected.Id);
                    };

                    selector.SelectionConfirmed += selection =>
                    {
                        var selected = selection.FirstOrDefault();
                        ValueEntry.SmartValue = selected;
                        selectedDefinition = GetDefinitionObject(selected.Id);
                    };
                }

                selector.OnInspectorGUI();
                selector.ShowInPopup();
            }

            EditorGUI.LabelField(controlRect, label);
            GUILayout.EndHorizontal();
        }

        private void DrawScriptableDef(GUIContent label)
        {
            var value = ValueEntry.SmartValue;

            var controlRect = EditorGUILayout.GetControlRect();

            label ??= GUIContent.none;

            if (scriptableObjectType == null)
            {
                scriptableObjectType = GetScriptableObjectType();
            }

            if (selectedDefinition == null && !string.IsNullOrEmpty(value.Id) || 
                selectedDefinition != null && selectedDefinition.DefId != value.Id)
            {
                selectedDefinition = GetDefinitionObject(value.Id);
            }

            EditorGUI.BeginChangeCheck();
            var labelWidth = label != GUIContent.none ? GUIHelper.BetterLabelWidth : 15;
            var rect = new Rect(controlRect.x + labelWidth, controlRect.y, controlRect.width - labelWidth,
                controlRect.height);

            var selected = EditorGUI
                    .ObjectField(rect, GUIContent.none, selectedDefinition, scriptableObjectType, false) as
                BaseScriptableDefinition;

            if (selected != selectedDefinition)
            {
                selectedDefinition = selected;
                var defRef = new DefRef<TDef>(selectedDefinition != null
                    ? selectedDefinition.DefId
                    : string.Empty);
                ValueEntry.SmartValue = defRef;

                UnityEngine.Object.DestroyImmediate(defEditor);
                defEditor = null;
            }

            UpdateDefEditor();
            showDefEditor = SirenixEditorGUI.Foldout(controlRect, showDefEditor && defEditor != null,
                label);
            if (SirenixEditorGUI.BeginFadeGroup(this, showDefEditor) && defEditor != null)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                defEditor.OnInspectorGUI();
                GUILayout.EndVertical();
            }

            SirenixEditorGUI.EndFadeGroup();
            EditorGUI.EndChangeCheck();
        }

        private void UpdateDefEditor()
        {
            if (selectedDefinition == null && defEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(defEditor);
                defEditor = null;
            }
            else if (selectedDefinition != null && defEditor == null)
            {
                defEditor = UnityEditor.Editor.CreateEditor(selectedDefinition);
                if (defEditor is ScriptableDefinitionDrawer or BlueprintObjectDrawer)
                {
                    ((BaseScriptableDefinitionDrawer)defEditor).SimpleMode = true;
                }
            }
        }

        private BaseScriptableDefinition GetDefinitionObject(string id)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(BaseScriptableDefinition)}");
            var assets = guids.Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<BaseScriptableDefinition>).ToList();
            var filteredAssets = assets
                .Where(e => e != null && e.DefId == id && e.DefinitionType == typeof(TDef)).ToList();

            if (filteredAssets.Count > 1)
            {
                Debug.LogError($"Multiple definitions with id {id} found. [ {string.Join(", ", filteredAssets.Select(e => e.name))} ]");
                return null;
            }

            return filteredAssets.FirstOrDefault();
        }

        private Type GetScriptableObjectType()
        {
            var objectType = typeof(ScriptableDefinition<>).MakeGenericType(typeof(TDef));
            var type = TypeUtils.AllTypes
                .FirstOrDefault(e => e.IsSubclassOf(objectType));

            return type;
        }
    }
}
