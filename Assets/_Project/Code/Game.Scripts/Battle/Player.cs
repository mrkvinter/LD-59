using System;
using System.Collections.Generic;
using System.Linq;
using Code.Game.Scripts.Battle.Items;
using Random = UnityEngine.Random;

namespace Code.Game.Scripts.Battle
{
    public class Player
    {
        public int Health { get; private set; }
        
        public readonly List<Sign> Signs = new();
        public readonly List<Sign> SignsBag = new();
        public readonly List<Sign> SignsHand = new();
        public readonly List<Item> Items = new();
        public readonly List<IAffectEnemySign> Affects = new();
        
        public Sign SelectedSign { get; private set; }
        public int RockProbability => (int)(AvailableSigns.Count(x => x == Sign.Rock) / (float) AvailableSigns.Count() * 100);
        public int PaperProbability => (int)((AvailableSigns.Count(x => x == Sign.Paper) / (float) AvailableSigns.Count()) * 100);
        public int ScissorsProbability => (int)((AvailableSigns.Count(x => x == Sign.Scissors) / (float) AvailableSigns.Count()) * 100);
        public int GoatProbability => (int)((AvailableSigns.Count(x => x == Sign.Goat) / (float) AvailableSigns.Count()) * 100);
        public int FProbability => (int)((AvailableSigns.Count(x => x == Sign.Fuck) / (float) AvailableSigns.Count()) * 100);

        public IEnumerable<Sign> AvailableSigns => 
            SignsHand.Where(x => Affects.All(a => a.IsSignAvailable(x)));
        
        public Player(int health, List<Sign> signsBag)
        {
            Health = health;
            SignsBag = signsBag.OrderBy(x => Guid.NewGuid()).ToList();
        }  
        
        public void Draw(int count)
        {
            while (count > 0 && SignsBag.Count > 0)
            {
                var sign = SignsBag[0];
                SignsBag.RemoveAt(0);
                SignsHand.Add(sign);
                count--;
            }
        }

        public void SelectSign()
        {
            SelectedSign = AvailableSigns.ToList()[Random.Range(0, AvailableSigns.Count())];
        }

        public void ReduceHealth()
        {
            Health--;
        }

        public void RemoveSelectedSign()
        {
            SignsHand.Remove(SelectedSign);
        }
        
        public void RemoveSign(Sign sign)
        {
            SignsHand.Remove(sign);
        }
        
        public void AddAffects(IAffectEnemySign affect)
        {
            Affects.Add(affect);

            if (!affect.IsSignAvailable(SelectedSign))
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