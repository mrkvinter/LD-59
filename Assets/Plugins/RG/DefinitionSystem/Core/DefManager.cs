using UnityEngine;

namespace RG.DefinitionSystem.Core
{
    public static class DefManager
    {
        private static DefinitionResolver definitionResolver;
        
        public static void Initialize(DefinitionResolver resolver)
        {
            if (definitionResolver != null)
            {
                Debug.LogError("DefManager already initialized. Are you calling Initialize twice?");
            }

            definitionResolver = resolver;
        }
        
        public static DefinitionMap<T> GetDefMap<T>() where T : Definition
        {
            return definitionResolver.GetDefMap<T>();
        }

        public static T GetDef<T>(string id) where T : Definition
        {
            return definitionResolver.GetDef<T>(id);
        }

        public static T GetDef<T>(DefRef<T> defRef) where T : Definition
        {
            return definitionResolver.GetDef(defRef);
        }

        public static T[] GetDefs<T>(DefRef<T>[] defRefs) where T : Definition
        {
            var result = new T[defRefs.Length];
            for (var i = 0; i < defRefs.Length; i++)
            {
                result[i] = definitionResolver.GetDef(defRefs[i]);
            }
            
            return result;
        }

        public static T GetSingletonDef<T>() where T : SingletonDefinition
        {
            return definitionResolver.GetSingletonDef<T>();
        }
    }
}