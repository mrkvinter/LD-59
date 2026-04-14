#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    public static class DefinitionDatabaseUtility
    {
        private static bool initialized;
        private static DefinitionDatabase definitionDatabase;

        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            definitionDatabase = AssetDatabaseUtility.GetAllAssetsOfType<DefinitionDatabase>().FirstOrDefault();

            if (definitionDatabase == null)
            {
                Debug.LogError("No definition database found");
            }
        }

        public static List<BaseScriptableSourceDefinition> GetSourceDefs<T>() where T : Definition
        {
            Initialize();

            var sourceDefs = new List<BaseScriptableSourceDefinition>();
            foreach (var sourceDef in definitionDatabase.Defs)
            {
                var defType = sourceDef.DefinitionType;
                if (typeof(T).IsAssignableFrom(defType))
                    sourceDefs.Add(sourceDef);
            }

            return sourceDefs;
        }
        
        public static List<T> GetDefinitions<T>() where T : Definition
        {
            Initialize();

            var definitions = new List<T>();
            foreach (var sourceDef in definitionDatabase.Defs)
            {
                if (sourceDef.DefinitionType == typeof(T))
                {
                    if (sourceDef is ScriptableDefinition<T> scriptableDef)
                    {
                        definitions.Add(scriptableDef.Definition);
                        continue;
                    }
                    
                    if (sourceDef is BaseScriptableTableDefinition<T> tableDef)
                    {
                        definitions.AddRange(tableDef.Definitions);
                        continue;
                    }
                }
            }

            return definitions;
        }

        public static List<Definition> GetDefinitions(Type type)
        {
            Initialize();

            return definitionDatabase.EnumerateDefinitions().Where(def => def.GetType() == type).ToList();
        }

        public static T GetDefinition<T>(DefRef<T> defRef) where T : Definition
        {
            Initialize();

            foreach (var sourceDef in definitionDatabase.Defs)
            {
                if (sourceDef.DefinitionType == typeof(T))
                {
                    if (sourceDef is ScriptableDefinition<T> scriptableDef 
                        && scriptableDef.DefId == defRef.Id)
                    {
                        return scriptableDef.Definition;
                    }
                    
                    if (sourceDef is BaseScriptableTableDefinition<T> tableDef)
                    {
                        foreach (var def in tableDef.Definitions)
                        {
                            if (def.Id == defRef.Id)
                                return def;
                        }
                    }
                }
            }

            return null;
        }

        public static Definition GetDefinition(string id)
        {
            Initialize();

            foreach (var sourceDef in definitionDatabase.Defs)
            {
                if (sourceDef is IScriptableDefinitionObject<Definition> scriptableDef 
                    && scriptableDef.Definition.Id == id)
                    return scriptableDef.Definition;
                
                if (sourceDef is IScriptableSourceDefinitions<Definition> tableDef)
                {
                    foreach (var def in tableDef.Definitions)
                    {
                        if (def.Id == id)
                            return def;
                    }
                }
            }

            return null;
        }

        public static DefAssetType GetAssetType<T>() where T : Definition
        {
            if (typeof(T).IsAbstract || typeof(T).IsInterface)
                return DefAssetType.TableDef;

            var objectType = typeof(ScriptableDefinition<>).MakeGenericType(typeof(T));
            if (TypeUtils.AllTypes.Any(e => e.IsSubclassOf(objectType)))
                return DefAssetType.ScriptableDef;

            objectType = typeof(BaseScriptableTableDefinition<>).MakeGenericType(typeof(T));
            if (TypeUtils.AllTypes.Any(e => e.IsSubclassOf(objectType)))
                return DefAssetType.TableDef;

            return DefAssetType.TableDef;
        }
    }

    public enum DefAssetType
    {
        None,
        ScriptableDef,
        TableDef
    }
}
#endif
