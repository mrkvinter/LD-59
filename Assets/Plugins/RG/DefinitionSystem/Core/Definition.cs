using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.Core
{
    [Serializable]
    public abstract class Definition
    {
        [field: SerializeField, HideIf(nameof(IsSettings))]
        public virtual string Id { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is Definition def && def.Id == Id && def.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GetType().Name} [{Id}]";
        }
        
        private bool IsSettings() => this is SingletonDefinition;
    }
}