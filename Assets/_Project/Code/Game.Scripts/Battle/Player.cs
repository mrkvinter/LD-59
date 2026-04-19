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

        public int WinStones { get; set; } = 0;

        public IEnumerable<Card> AvailableSigns => 
            CardsHand.Where(x => Affects.All(a => a.IsSignAvailable(x)));
        
        public Player(int health, List<DefRef<SignDef>> signsBag)
        {
            Health = health;

            Cards.AddRange(signsBag.Select(x => new Card(x.Unwrap())));
            foreach (var signDef in signsBag)
            {
                CardsBag.Add(new Card(signDef.Unwrap()));
            }
        }

        public void ReplaceBag(List<DefRef<SignDef>> signsBag)
        {
            Cards.Clear();
            CardsBag.Clear();
            
            Cards.AddRange(signsBag.Select(x => new Card(x.Unwrap())));
            CardsBag.AddRange(Cards);
        }

        public void RefillBag()
        {
            CardsBag.Clear();
            CardsBag.AddRange(Cards);
        }

        public void Draw(int count)
        {
            while (count > 0 && CardsBag.Count > 0)
            {
                var index = Random.Range(0, CardsBag.Count);
                var sign = CardsBag[index];
                CardsBag.RemoveAt(index);
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

        public void AddHealth(int count)
        {
            Health += count;
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

        public void ClearHand(CardHolder cardHolder)
        {
            foreach (var card in CardsHand)
            {
                cardHolder.Remove(card.View);
                Object.Destroy(card.View.gameObject);
            }
            
            CardsHand.Clear();
        }
    }
}