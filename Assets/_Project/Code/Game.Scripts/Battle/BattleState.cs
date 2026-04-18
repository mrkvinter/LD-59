using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Game.Core;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Code.Game.Scripts.Battle
{
    public class BattleState
    {
        private const int Rounds = 3;
        private const int SignsPerRound = 5;
        
        private int currentRound;
        private Player enemyPlayer;
        private Player player;

        private SceneLinks sceneLinks = G.Resolve<SceneLinks>();

        public void OnEnter()
        {
            currentRound = 0;
            var baseDeck = new List<Sign>();
            baseDeck.AddRange(Enumerable.Repeat(Sign.Rock, 3));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Paper, 3));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Scissors, 3));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Goat, 1));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Fuck, 1));
            
            enemyPlayer = new Player(3, baseDeck.ToList());
            player = new Player(3, baseDeck.ToList());
            
            sceneLinks.EnemyHealthPanel.SetHealthCount(player.Health);
            sceneLinks.PlayerHealthPanel.SetHealthCount(enemyPlayer.Health);
            
            StartRound();
        }

        public void OnExit()
        {
            
        }

        private void StartRound()
        {
            currentRound++;
            enemyPlayer.Draw(SignsPerRound);
            player.Draw(SignsPerRound);
            PrintEnemyState();
            
            enemyPlayer.SelectSign();
            
            InstantiateCards();
        }

        private void PrintEnemyState()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Rock: {enemyPlayer.RockProbability}%")
                .AppendLine($"Paper: {enemyPlayer.PaperProbability}%")
                .AppendLine($"Scissors: {enemyPlayer.ScissorsProbability}%")
                .AppendLine($"Goat: {enemyPlayer.GoatProbability}%")
                .AppendLine($"F@3k: {enemyPlayer.FProbability}%");
            
            sceneLinks.EnemyStateText.text = sb.ToString();
        }

        private void InstantiateCards()
        {
            foreach (var sign in player.SignsHand)
            {
                var card = Object.Instantiate(sceneLinks.CardPrefab, sceneLinks.CardsParent);
                card.SetSign(sign);
                
                card.OnClick += () => OnCardSelect(card);
            }
        }

        private void OnCardSelect(CardView cardView) => UniTask.Create(async () =>
        {
            Object.Destroy(cardView.gameObject);
            sceneLinks.Hands.SetActive(true);
            sceneLinks.GameUI.SetActive(false);

            sceneLinks.LeftHandView.SetSign(Sign.Rock);
            sceneLinks.RightHandView.SetSign(Sign.Rock);

            sceneLinks.LeftHandView.PlayShakeAnimation().Forget();
            await sceneLinks.RightHandView.PlayShakeAnimation();
            
            sceneLinks.LeftHandView.SetSign(enemyPlayer.SelectedSign);
            sceneLinks.RightHandView.SetSign(cardView.SelectedSign);
            
            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            
            var winner = GetWinner(enemyPlayer.SelectedSign, cardView.SelectedSign);
            
            if (winner == Winner.Right)
            {
                sceneLinks.WinTitle.SetActive(true);
                enemyPlayer.ReduceHealth();
            }
            if (winner == Winner.Left)
            {
                sceneLinks.LoseTitle.SetActive(true);
                player.ReduceHealth();
            }
            if (winner == Winner.Draw) sceneLinks.DrawTitle.SetActive(true);
            
            enemyPlayer.RemoveSelectedSign();
            player.RemoveSign(cardView.SelectedSign);

            sceneLinks.EnemyHealthPanel.SetHealthCount(enemyPlayer.Health);
            sceneLinks.PlayerHealthPanel.SetHealthCount(player.Health);
            PrintEnemyState();

            await UniTask.Delay(TimeSpan.FromSeconds(4f));
            
            sceneLinks.WinTitle.SetActive(false);
            sceneLinks.LoseTitle.SetActive(false);
            sceneLinks.DrawTitle.SetActive(false);

            sceneLinks.Hands.SetActive(false);
            sceneLinks.GameUI.SetActive(true);
            
            enemyPlayer.SelectSign();
        });

        private Winner GetWinner(Sign leftSign, Sign rightSign)
        {
            var leftSignDef = SignDef.Get(leftSign);
            var rightSignDef = SignDef.Get(rightSign);
            var isLeftWin = leftSignDef.BeatSigns.Contains(rightSignDef.Sign);
            var isRightWin = rightSignDef.BeatSigns.Contains(leftSignDef.Sign);
            
            return (isLeftWin, isRightWin) switch
            {
                (true, true) => Winner.Draw,
                (true, false) => Winner.Left,
                (false, true) => Winner.Right,
                _ => Winner.Draw
            };
        }
    }

    public enum Winner
    {
        Left,
        Right,
        Draw
    }

    public class Player
    {
        public int Health { get; private set; }
        
        public readonly List<Sign> Signs = new();
        public readonly List<Sign> SignsBag = new();
        public readonly List<Sign> SignsHand = new();
        
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

    public enum Sign
    {
        None,
        Rock,
        Paper,
        Scissors,
        Goat,
        Fuck
    }
}