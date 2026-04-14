using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RG.DefinitionSystem.Core;
using RG.DefinitionSystem.UnityAdapter;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace RG.DefinitionSystem.Editor
{
    public sealed class  DefSelector<TDef> : OdinSelector<DefRef<TDef>>
        where TDef : Definition
    {
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            var sourceDefs = DefinitionDatabaseUtility.GetSourceDefs<TDef>();
            tree.Selection.SupportsMultiSelect = false;
            tree.Add("None", new DefRef<TDef>(string.Empty));

            foreach (var (path, def) in JsonDefinitionConfigManager.Instance.GetDefinitions<TDef>())
            {
                tree.Add(Path.GetFileNameWithoutExtension(path), new DefRef<TDef>(def.Id));
            }

            foreach (var sourceDef in sourceDefs)
            {
                if (sourceDef is IScriptableDefinitionObject<TDef> scriptableDef)
                {
                    var path = $"{scriptableDef.DefId}_{scriptableDef.DefinitionType.Name}";
                    tree.Add(path, new DefRef<TDef>(scriptableDef.DefId));
                    continue;
                }
                
                if (sourceDef is Blueprint blueprint && blueprint.Definition is TDef)
                {
                    var path = $"{blueprint.DefId}_{typeof(TDef).Name}";
                    tree.Add(path, new DefRef<TDef>(blueprint.DefId));
                    continue;
                }

                if (sourceDef is BaseScriptableTableDefinition<TDef> tableDef)
                {
                    const string prefixToRemove = "Assets/";
                    var rootPath = AssetDatabase.GetAssetPath(tableDef);
                    if (rootPath.StartsWith(prefixToRemove))
                        rootPath = rootPath[prefixToRemove.Length..];
                    rootPath = rootPath.Replace(".asset", string.Empty);

                    for (var i = 0; i < tableDef.Definitions.Length; i++)
                    {
                        var def = tableDef.Definitions[i];
                        var path = $"{rootPath}/{def.Id}";
                        tree.Add(path, new DefRef<TDef>(def.Id));
                    }
                    continue;
                }
            }
        }

        private void CutCommonPrefixTree(List<(string path, DefRef<TDef> def)> tree)
        {
            var minLen = int.MaxValue;
            foreach (var (path, _) in tree)
            {
                minLen = Mathf.Min(minLen, path.Length);
            }

            var postFixIndexToRemove = 0;
            for (int i = 0; i < minLen; i++)
            {
                if (tree.Any(x => x.path[0] != x.path[i]))
                    postFixIndexToRemove = i;
            }
            
            for (var i = 0; i < tree.Count; i++)
            {
                tree[i] = (tree[i].path[postFixIndexToRemove..], tree[i].def);
            }
        }

        public void ClearSubsribtions()
        {
            
        }
    }
}
