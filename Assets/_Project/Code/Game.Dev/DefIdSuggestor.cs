using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QFSW.QC;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace Game.Core.ContentIdSuggestors
{
    public struct ContentIdTag : IQcSuggestorTag
    {
    }

    public sealed class ContentIdAttribute : SuggestorTagAttribute
    {
        private readonly IQcSuggestorTag[] _tags = { new ContentIdTag() };

        public override IQcSuggestorTag[] GetSuggestorTags()
        {
            return _tags;
        }
    }

    public class DefIdSuggestor : IQcSuggestor
    {
        private List<IDefIdSuggesterFilter> filters;
        
        public IEnumerable<IQcSuggestion> GetSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            InitFilters();
            var targetType = context.TargetType;

            if (typeof(Definition).IsAssignableFrom(targetType))
            {
                return DefManager.GetDefMap<Definition>().DefinitionsEntries
                    .Where(e => e.GetType() == targetType)
                    .Where(e => e.Id.ToLower().Contains(context.Prompt.ToLower()))
                    .Where(e => filters.All(f => f.Filter(e)))
                    .Select(e => new RawSuggestion(e.Id));
            }
            
            return Enumerable.Empty<IQcSuggestion>();
        }

        private void InitFilters()
        {
            if (filters != null) return;

            var interfaceType = typeof(IDefIdSuggesterFilter);
            filters = new List<IDefIdSuggesterFilter>();
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
                })
                .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToList();

            foreach (var type in types)
            {
                try
                {
                    if (Activator.CreateInstance(type) is IDefIdSuggesterFilter instance)
                    {
                        filters.Add(instance);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create filter {type.Name}: {e}");
                }
            }
        }
        public interface IDefIdSuggesterFilter
        {
            bool Filter(Definition def);
        }
    }
    
    public class DefIdParser : PolymorphicQcParser<Definition>
    {
        public override Definition Parse(string value, Type type)
        {
            value = value.Replace("\"", "");
            return DefManager.GetDefMap<Definition>().DefinitionsEntries.FirstOrDefault(e => e.GetType() == type && e.Id == value);
        }
    }
}