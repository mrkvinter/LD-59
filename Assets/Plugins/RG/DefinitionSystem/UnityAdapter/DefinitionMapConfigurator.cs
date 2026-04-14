using System.Collections.Generic;
using RG.DefinitionSystem.Core;

namespace RG.DefinitionSystem.UnityAdapter
{
    public interface IDefinitionMapConfigurator
    {
        IDefinitionMap Configure(IReadOnlyCollection<BaseScriptableSourceDefinition> sourceDefs,
            Definition[] definitionDatabaseDefinitions);
        IDefinitionMap Configure(IReadOnlyCollection<IDefinitionMap> maps);
    }

    public class DefinitionMapConfigurator<T> : IDefinitionMapConfigurator
        where T : Definition
    {
        public IDefinitionMap Configure(IReadOnlyCollection<BaseScriptableSourceDefinition> sourceDefs,
            Definition[] definitionDatabaseDefinitions)
        {
            var result = new List<T>();
            foreach (var sourceDef in sourceDefs)
            {
                if (sourceDef == null)
                    throw new System.Exception("Source definition is null");

                if (sourceDef is IScriptableDefinitionObject<T> scriptableDef)
                    result.Add(scriptableDef.Definition);

                if (sourceDef is Blueprint blueprint && blueprint.Definition is T def)
                    result.Add(def);

                if (sourceDef is IScriptableSourceDefinitions<T> scriptableSourceDefs)
                    result.AddRange(scriptableSourceDefs.Definitions);
            }
            
            foreach (var def in definitionDatabaseDefinitions)
            {
                if (def is T casted)
                    result.Add(casted);
            }

            return new DefinitionMap<T>(result);
        }

        public IDefinitionMap Configure(IReadOnlyCollection<IDefinitionMap> maps)
        {
            var result = new List<T>();
            foreach (var map in maps)
            {
                if (map is IDefinitionMap<T> casted)
                    result.AddRange(casted.DefinitionsEntries);
            }

            return new DefinitionMap<T>(result);
        }
    }
}
