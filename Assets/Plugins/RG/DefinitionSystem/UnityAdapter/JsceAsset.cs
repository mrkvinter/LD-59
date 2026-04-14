using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    /// <summary>
    /// Asset container для .def файлов
    /// </summary>
    public class JsceAsset : BaseScriptableSourceDefinition, IScriptableDefinitionObject<Definition>
    {
        [SerializeField, HideInInspector]
        private string type;

        [SerializeReference, InlineProperty, HideReferenceObjectPicker, HideLabel]
        private Definition entry;

        [SerializeField, HideInInspector]
        public string jsonText;

        [SerializeField, HideInInspector]
        public string importError;

        public string Type => type;
        public Definition Entry => entry;
        public override System.Type DefinitionType => entry?.GetType();
        public Definition Definition => entry;
        public string DefId => entry?.Id;
    }
}
