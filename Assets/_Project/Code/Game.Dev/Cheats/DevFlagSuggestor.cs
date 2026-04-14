using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Code.Prefs;
using QFSW.QC;

namespace Code.Cheats
{
    public struct DevFlagNameTag : IQcSuggestorTag
    {

    }

    public sealed class DevFlagNameAttribute : SuggestorTagAttribute
    {
        private readonly IQcSuggestorTag[] _tags = { new DevFlagNameTag() };

        public override IQcSuggestorTag[] GetSuggestorTags()
        {
            return _tags;
        }
    }

    public class DevFlagSuggestor : BasicCachedQcSuggestor<string>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            return context.HasTag<DevFlagNameTag>();
        }

        protected override IQcSuggestion ItemToSuggestion(string abilityName)
        {
            return new RawSuggestion(abilityName, true);
        }

        protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return typeof(GameCheatFlags).GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.FieldType == typeof(BoolPlayerPref))
                .Select(e => e.Name)
                .ToArray();
        }
    }
}