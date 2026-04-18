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
        
        public Sign SelectedSign { get; private set; }
        public int RockProbability => (int)(SignsHand.Count(x => x == Sign.Rock) / (float) SignsHand.Count * 100);
        public int PaperProbability => (int)((SignsHand.Count(x => x == Sign.Paper) / (float) SignsHand.Count) * 100);
        public int ScissorsProbability => (int)((SignsHand.Count(x => x == Sign.Scissors) / (float) SignsHand.Count) * 100);
        public int GoatProbability => (int)((SignsHand.Count(x => x == Sign.Goat) / (float) SignsHand.Count) * 100);
        public int FProbability => (int)((SignsHand.Count(x => x == Sign.Fuck) / (float) SignsHand.Count) * 100);

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
            SelectedSign = SignsHand[Random.Range(0, SignsHand.Count)];
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
    }
}