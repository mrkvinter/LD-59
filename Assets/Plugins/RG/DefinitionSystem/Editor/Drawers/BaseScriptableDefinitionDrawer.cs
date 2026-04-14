using System.Linq;
using RG.DefinitionSystem.Core.Constants;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor.Drawers
{
    public abstract class BaseScriptableDefinitionDrawer : OdinEditor
    {
        public bool SimpleMode { get; set; }

        public override void OnInspectorGUI()
        {
            var scriptableDefinition = (BaseScriptableSourceDefinition)serializedObject.targetObject;
            Draw(scriptableDefinition);
        }
        
        protected virtual void Draw(BaseScriptableSourceDefinition scriptableDefinition)
        {
            if (!SimpleMode)
            {
                GUI.enabled = false;
                SirenixEditorFields.UnityObjectField(GUIContent.none, scriptableDefinition,
                    typeof(BaseScriptableSourceDefinition), false);
                GUI.enabled = true;
            }

            GUILayout.Space(10);

            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (!SimpleMode && scriptableDefinition.DefinitionType.GetCustomAttributes(true)
                    .Any(e => e is WithConstantsAttribute) &&
                GUILayout.Button("Generate types"))
            {
                GenerateConstants(scriptableDefinition);
            }
        }

        protected void GenerateConstants(BaseScriptableSourceDefinition scriptableDefinition)
        {
            var constsGenerator = new EntryConstantsClassGenerator(scriptableDefinition);
            constsGenerator.Generate();
        }
    }
}