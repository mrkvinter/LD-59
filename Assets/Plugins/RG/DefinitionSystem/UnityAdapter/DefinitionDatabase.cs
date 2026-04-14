using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RG.DefinitionSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    [CreateAssetMenu(fileName = "DefinitionDatabase", menuName = "Content/DefinitionDatabase")]
    public sealed class DefinitionDatabase : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public BaseScriptableSourceDefinition[] Defs { get; private set; }
        [field: SerializeReference, ReadOnly] public Definition[] Definitions { get; private set; }

        public IEnumerable<Definition> EnumerateDefinitions()
        {
            foreach (var sourceDefinition in Defs)
            {
                if (sourceDefinition is IScriptableDefinitionObject<Definition> scriptableDef)
                    yield return scriptableDef.Definition;
                
                if (sourceDefinition is IScriptableSourceDefinitions<Definition> scriptableSourceDefs)
                    foreach (var def in scriptableSourceDefs.Definitions)
                        yield return def;
            }
            
            foreach (var def in Definitions)
                yield return def;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            JsonDefinitionConfigManager.Instance.Changed += OnExternalDefinitionsChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            JsonDefinitionConfigManager.Instance.Changed -= OnExternalDefinitionsChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnExternalDefinitionsChanged()
        {
            JsonDefinitionConfigManager.Instance.LoadAllDefinitions();
            Definitions = JsonDefinitionConfigManager.Instance.CachedDefinitions.Values
                .Select(e => e.Definition)
                .Where(d => d is { Id: not null })
                .ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button(ButtonSizes.Medium)]
        public void Populate()
        {
            var directory = new FileInfo(UnityEditor.AssetDatabase.GetAssetPath(this)).Directory;
            var defPaths = directory!.GetDirectories().SelectMany(GetAllFiles)
                .Select(e =>
                    $"Assets/{e.FullName.Replace('\\', '/').Replace(Application.dataPath, string.Empty)}")
                .ToHashSet();

            var usedId = new HashSet<Key>();
            var defs = new List<BaseScriptableSourceDefinition>();
            foreach (var defPath in defPaths)
            {
                var sourceDef = UnityEditor.AssetDatabase.LoadAssetAtPath<BaseScriptableSourceDefinition>(defPath);
                if (sourceDef == null)
                    continue;

                if (sourceDef is IScriptableDefinitionObject<Definition> scriptableDef)
                {
                    if (string.IsNullOrEmpty(scriptableDef.Definition.Id))
                    {
                        Debug.LogError($"Definition {defPath} has no id", sourceDef);
                        return;
                    }

                    var key = new Key(scriptableDef.Definition.Id, scriptableDef.DefinitionType);
                    if (usedId.Contains(key))
                    {
                        Debug.LogError(
                            $"Definition {defPath} has duplicate id {scriptableDef.Definition.Id}", sourceDef);
                        return;
                    }

                    usedId.Add(key);
                    defs.Add(sourceDef);
                }

                if (sourceDef is IScriptableSourceDefinitions<Definition> scriptableSourceDefs)
                {
                    foreach (var def in scriptableSourceDefs.Definitions)
                    {
                        if (string.IsNullOrEmpty(def.Id))
                        {
                            Debug.LogError($"Definition {defPath} has no id", sourceDef);
                            return;
                        }

                        var key = new Key(def.Id, scriptableSourceDefs.DefinitionType);
                        if (usedId.Contains(key))
                        {
                            Debug.LogError(
                                $"Definition {defPath} has duplicate id {def.Id}", sourceDef);
                            return;
                        }

                        usedId.Add(key);
                    }
                    
                    defs.Add(sourceDef);
                }
            }

            Defs = defs.ToArray();

            OnExternalDefinitionsChanged();
        }

        private IEnumerable<FileInfo> GetAllFiles(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles())
            {
                yield return file;
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                foreach (var file in GetAllFiles(subDirectory))
                {
                    yield return file;
                }
            }
        }
        
        private struct Key : IEquatable<Key>
        {
            public string Id;
            public Type DefType;

            public Key(string id, Type defType)
            {
                Id = id;
                DefType = defType;
            }

            public bool Equals(Key other)
            {
                return Id == other.Id && Equals(DefType, other.DefType);
            }

            public override bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, DefType);
            }
        }
#endif
    }
}