using System;
using System.Collections.Generic;
using System.Linq;
using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    [Obsolete("Use DefinitionJson instead")]
    [CreateAssetMenu(menuName = "Content/Blueprint")]
    public sealed class Blueprint : BaseScriptableSourceDefinition, IScriptableDefinitionObject<Definition>
    {
        [SerializeField, HideInInspector] private string type;
        [SerializeReference, InlineProperty, HideReferenceObjectPicker, HideLabel] private Definition entry;

        public string Type
        {
            get => type;
            set
            {
                cachedType = null;
                type = value;
            }
        }

        public Definition Entry { get => entry; set => entry = value; }

        public override Type DefinitionType => cachedType ??= GetDefinitionType();
        public Definition Definition => Entry;
        public string DefId => Entry?.Id;

        private Type cachedType;
        private static Dictionary<string, Type> allTypes;

        private Type GetDefinitionType()
        {
            allTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => typeof(Definition).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.FullName);
            return string.IsNullOrEmpty(Type) ? null : allTypes.GetValueOrDefault(Type);
        }
    }
}