using System;
using System.Collections.Generic;
using System.Linq;

namespace RG.DefinitionSystem.UnityAdapter
{
    public static class TypeUtils
    {
        private static List<Type> _cachedTypes;

        public static IReadOnlyList<Type> AllTypes => _cachedTypes ??= GetAllTypes();

        private static List<Type> GetAllTypes() => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .ToList();
    }
}