using System;
using System.Collections.Generic;

namespace RG.DefinitionSystem.Core
{
    public sealed class DefinitionResolver
    {
        private readonly Dictionary<Type, IDefinitionMap> maps;

        public DefinitionResolver(IEnumerable<IDefinitionMap> defMaps)
        {
            maps = new Dictionary<Type, IDefinitionMap>();
            foreach (var map in defMaps)
            {
                maps[map.DefinitionType] = map;
            }
        }

        public DefinitionMap<T> GetDefMap<T>() where T : Definition
        {
            return (DefinitionMap<T>) maps[typeof(T)];
        }

        public T GetDef<T>(string id) where T : Definition
        {
            return GetDefMap<T>().GetDef(id);
        }

        public T GetDef<T>(DefRef<T> defRef) where T : Definition
        {
            return GetDefMap<T>().GetDef(defRef.Id);
        }

        public T GetSingletonDef<T>() where T : SingletonDefinition
        {
            return (T) GetDefMap<SingletonDefinition>().GetDef(typeof(T).Name);
        }
    }
}