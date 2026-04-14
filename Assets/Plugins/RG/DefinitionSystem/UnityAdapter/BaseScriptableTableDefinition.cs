using System;
using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    /// <summary>
    /// For source that represents a list of definitions
    /// </summary>
    /// <typeparam name="T">The type of the definition</typeparam>
    public interface IScriptableSourceDefinitions<out T> where T : Definition
    {
        Type DefinitionType { get; }
        T[] Definitions { get; }
    }

    public abstract class BaseScriptableTableDefinition : BaseScriptableSourceDefinition
    {
    }

    public abstract class BaseScriptableTableDefinition<T> : BaseScriptableTableDefinition,
        IScriptableSourceDefinitions<T>
        where T : Definition
    {
        public override Type DefinitionType => typeof(T);

        [field: SerializeField, TableList] public T[] Definitions { get; private set; }
    }
}