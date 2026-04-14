using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    /// <summary>
    /// For source that represents a single definition
    /// </summary>
    /// <typeparam name="T">The type of the definition</typeparam>
    public interface IScriptableDefinitionObject<out T> where T : Definition
    {
        Type DefinitionType { get; }
        T Definition { get; }
        string DefId { get; }
    }

    public interface IProcessSelfAttributes
    {
        IEnumerable<Attribute> ProcessSelfAttributes(string property);
    }

    public abstract class BaseScriptableDefinition : BaseScriptableSourceDefinition
    {
        public abstract string DefId { get; }
    }

    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public abstract class ScriptableDefinition<T> : BaseScriptableDefinition, IScriptableDefinitionObject<T>, IProcessSelfAttributes,
        ISelfValidator
        where T : Definition
    {
        [SerializeField] [InlineProperty, HideLabel]
        private T definition;

        public T Definition => definition;

        public override string DefId => definition.Id;
        public override Type DefinitionType => typeof(T);

        public virtual IEnumerable<Attribute> ProcessSelfAttributes(string property)
        {
            if (property == nameof(Core.Definition.Id) && DefinitionType.IsSubclassOf(typeof(SingletonDefinition)))
            {
                yield return new HideInInspector();
                yield break;
            }

            foreach (var processAttribute in ProcessAttributes(property))
            {
                yield return processAttribute;
            }
        }

        protected virtual IEnumerable<Attribute> ProcessAttributes(string property)
        {
            yield break;
        }

        protected virtual void OnValidated(SelfValidationResult result)
        {
        }

        public void Validate(SelfValidationResult result)
        {
            if (!typeof(T).GetCustomAttributes().Any(e => e is SerializableAttribute))
            {
                result.AddError($"{typeof(T).Name} must be marked as [Serializable]");
            }

            OnValidated(result);
        }
    }
}