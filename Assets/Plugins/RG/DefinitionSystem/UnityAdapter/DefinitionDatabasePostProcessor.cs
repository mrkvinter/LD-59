#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;

namespace RG.DefinitionSystem.UnityAdapter
{
    public class DefinitionDatabasePostProcessor : UnityEditor.AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var needPopulate = false;

            foreach (var assetPath in importedAssets.Concat(deletedAssets).Concat(movedAssets))
            {
                if (assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Blueprint>(assetPath);
                    if (asset != null)
                    {
                        needPopulate = true;
                        break;
                    }
                }
            }
            
            if (!needPopulate)
                return;

            var definitionDatabase = Resources.Load<DefinitionDatabase>("DefinitionDatabase");
            if (definitionDatabase != null)
            {
                definitionDatabase.Populate();
            }
        }
    }
}
#endif