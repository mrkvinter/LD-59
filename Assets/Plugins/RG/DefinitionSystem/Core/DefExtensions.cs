using System;

namespace RG.DefinitionSystem.Core
{
    public static class DefExtensions
    {
        public static T As<T>(this string id) where T : Definition
        {
            return DefManager.GetDef<T>(id);
        }

        public static T Unwrap<T>(this DefRef<T> defRef) where T : Definition
        {
            return DefManager.GetDef(defRef);
        }

        public static T[] Unwrap<T>(this DefRef<T>[] defRefs) where T : Definition
        {
            if (defRefs == null)
            {
                return Array.Empty<T>();
            }

            var result = new T[defRefs.Length];
            for (var i = 0; i < defRefs.Length; i++)
            {
                result[i] = defRefs[i].Unwrap();
            }

            return result;
        }
    }
}