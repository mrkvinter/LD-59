using System;
using System.Collections.Generic;

namespace RG.DefinitionSystem.Core
{
    public interface IDefinitionMap
    {
        Type DefinitionType { get; }
    }

    public interface IDefinitionMap<out T> : IDefinitionMap
    {
        IReadOnlyList<T> DefinitionsEntries { get; }
    }

    public sealed class DefinitionMap<T> : IDefinitionMap<T>
        where T : Definition
    {
        private readonly Dictionary<string, T> definitionById = new();
        private readonly List<T> definitions;

        public IReadOnlyList<T> DefinitionsEntries => definitions;
        public Type DefinitionType => typeof(T);

        public DefinitionMap(IReadOnlyCollection<T> definitions)
        {
            this.definitions = new List<T>(definitions);
            foreach (var def in definitions)
            {
                definitionById[def.Id] = def;
            }
        }

        public T GetDef(string id)
        {
            return definitionById[id];
        }

        public bool TryGetDef(string id, out T def)
        {
            return definitionById.TryGetValue(id, out def);
        }
    }
}