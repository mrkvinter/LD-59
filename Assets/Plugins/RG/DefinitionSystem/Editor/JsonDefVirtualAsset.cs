using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector;

namespace RG.DefinitionSystem.Editor
{
    /// <summary>
    /// Виртуальный ассет для редактирования .def файлов вне папки Assets
    /// </summary>
    public class JsonDefVirtualAsset : BaseScriptableSourceDefinition, IScriptableDefinitionObject<Definition>
    {
        [Title("Asset Properties")]
        [ReadOnly]
        [HideInInspector]
        public string FilePath;

        [SerializeField, HideInInspector] private string type;
        [SerializeReference, InlineProperty, HideReferenceObjectPicker, HideLabel] private Definition entry;

        public string Type
        {
            get => type;
            private set
            {
                cachedType = null;
                type = value;
            }
        }

        public string TypeShortName => Type?.Split('.').Last();

        public Definition Entry { get => entry;
            private set => entry = value; }

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

        internal void Initialize(string path, string type, Definition entry)
        {
            FilePath = path;
            Type = type;
            Entry = entry;
            name = System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}
