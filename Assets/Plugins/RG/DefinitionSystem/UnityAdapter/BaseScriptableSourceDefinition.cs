using System;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    public abstract class BaseScriptableSourceDefinition : ScriptableObject
    {
        public abstract Type DefinitionType { get; }
    }
}