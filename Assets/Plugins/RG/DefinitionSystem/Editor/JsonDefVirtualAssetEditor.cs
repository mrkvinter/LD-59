using RG.DefinitionSystem.Editor.Drawers;
using RG.DefinitionSystem.UnityAdapter;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    [CustomEditor(typeof(JsonDefVirtualAsset))]
    public class JsonDefVirtualAssetEditor : BaseScriptableDefinitionDrawer
    {
        private const float SaveDelay = 0.5f;
        private double lastChangeTime;
        private bool needsSave;
        private JsonDefVirtualAsset virtualAsset;
        private GUIStyle tIconStyle;
        private Color tIconColor = ColorUtility.TryParseHtmlString("#7fd6fc", out var color) ? color : Color.white;

        protected override void OnEnable()
        {
            base.OnEnable();
            needsSave = false;
            EditorApplication.update += OnUpdate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= OnUpdate;
            if (needsSave && virtualAsset != null)
            {
                SaveNow();
            }
        }

        private void OnUpdate()
        {
            if (needsSave && EditorApplication.timeSinceStartup - lastChangeTime > SaveDelay)
            {
                SaveNow();
            }
        }

        private void SaveNow()
        {
            if (virtualAsset != null && !string.IsNullOrEmpty(virtualAsset.FilePath))
            {
                JsonDefinitionConfigManager.Instance.SaveFile(virtualAsset.FilePath, virtualAsset.Type, virtualAsset.Entry);
            }
            needsSave = false;
        }

        protected override void Draw(BaseScriptableSourceDefinition scriptableDefinition)
        {
            virtualAsset = (JsonDefVirtualAsset) scriptableDefinition;

            if (virtualAsset.Entry == null)
            {
                EditorGUILayout.HelpBox("Entry is null", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();

            DrawDefinitionType();
            
            base.Draw(scriptableDefinition);

            if (EditorGUI.EndChangeCheck() || serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                lastChangeTime = EditorApplication.timeSinceStartup;
                needsSave = true;
                
                EditorUtility.SetDirty(virtualAsset);
            }
        }

        private void DrawDefinitionType()
        {
            GUI.enabled = false;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.objectField))
            {
                tIconStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, 
                    normal = { textColor = tIconColor },
                    hover = { textColor = tIconColor },
                    active = { textColor = tIconColor }
                };
                GUILayout.Label("T", tIconStyle, GUILayout.Width(10));

                GUILayout.Label(new GUIContent(virtualAsset.TypeShortName, virtualAsset.Type), EditorStyles.label);
            }
            GUI.enabled = true;
        }
    }
}
