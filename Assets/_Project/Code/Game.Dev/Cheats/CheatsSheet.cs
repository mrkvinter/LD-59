using Game.Core.ContentIdSuggestors;
using QFSW.QC;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace Code.Cheats
{
    public static class CheatsSheet
    {
        [Command("game.test")]
        public static void AddDice(int number)
        {
            Debug.Log($"Adding {number} dice");
        }
        
    }
    
    public class DiceContentIdSuggesterFilter : DefIdSuggestor.IDefIdSuggesterFilter
    {
        public bool Filter(Definition entry) => true;
    }
}