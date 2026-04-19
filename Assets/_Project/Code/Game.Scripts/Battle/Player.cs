using System.Collections.Generic;
using System.Linq;
using Code.Game.Scripts.Battle.Items;
using RG.DefinitionSystem.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Game.Scripts.Battle
{
    public class Player
    {
        public int Health { get; private set; }
        
        public readonly List<Card> Cards = new();
        public readonly List<Card> CardsBag = new();
        public readonly List<Card> CardsHand = new();
        public readonly List<Item> Items = new();
        public readonly List<IAffectEnemySign> Affects = new();
        
        public Card SelectedCard { get; private set; }
        // public int RockProbability => (int)(AvailableSigns.Count(x => x == Sign.Rock) / (float) AvailableSigns.Count() * 100);
        // public int PaperProbability => (int)((AvailableSigns.Count(x => x == Sign.Paper) / (float) AvailableSigns.Count()) * 100);
        // public int ScissorsProbability => (int)((AvailableSigns.Count(x => x == Sign.Scissors) / (float) AvailableSigns.Count()) * 100);
        // public int GoatProbability => (int)((AvailableSigns.Count(x => x == Sign.Goat) / (float) AvailableSigns.Count()) * 100);
        // public int FProbability => (int)((AvailableSigns.Count(x => x == Sign.Fuck) / (float) AvailableSigns.Count()) * 100);

        public IEnumerable<Card> AvailableSigns => 
            CardsHand.Where(x => Affects.All(a => a.IsSignAvailable(x)));
        
        public Player(int health, List<DefRef<SignDef>> signsBag)
        {
            Health = health;

            foreach (var signDef in signsBag)
            {
                CardsBag.Add(new Card(signDef.Unwrap()));
            }
        }  
        
        public void Draw(int count)
        {
            while (count > 0 && CardsBag.Count > 0)
            {
                var sign = CardsBag[0];
                CardsBag.RemoveAt(0);
                CardsHand.Add(sign);
                count--;
            }
        }

        public void SelectSign()
        {
            var cards = CardsHand.Where(e => !e.IsBLocked).ToList();
            if (cards.Count == 0)
            {
                SelectedCard = null;
                return;
            }
            SelectedCard = cards[Random.Range(0, cards.Count())];
        }

        public void ReduceHealth(int count)
        {
            Health -= count;
            
            Health = Mathf.Max(Health, 0);
        }

        public void RemoveSelectedSign()
        {
            CardsHand.Remove(SelectedCard);
            SelectedCard = null;
        }
        
        public void RemoveCard(Card card)
        {
            CardsHand.Remove(card);
        }
        
        public void AddAffects(IAffectEnemySign affect)
        {
            Affects.Add(affect);

            if (!affect.IsSignAvailable(SelectedCard))
            {
                RemoveSelectedSign();
                SelectSign();
            }
        }
        
        public void ClearAffects()
        {
            Affects.Clear();
        } 
    }
}