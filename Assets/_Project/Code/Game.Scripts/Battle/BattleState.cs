using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Game.Core;
using Code.Game.Scripts.Battle.Items;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RG.DefinitionSystem.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Scripts.Battle
{
    public class BattleState
    {
        private const int Rounds = 3;
        private const int SignsPerRound = 5;
        
        private int currentRound;
        private Player enemyPlayer;
        private Player player;

        private readonly SceneLinks sceneLinks = G.Resolve<SceneLinks>();
        private readonly ItemsService itemsService;
        
        private List<IAffectGame> affectGames = new();

        public BattleState()
        {
            itemsService = new ItemsService();

            AddItem(ItemDefType.BrokenGlass);
            AddItem(ItemDefType.Knife);
        }

        private void AddItem(DefRef<ItemDef> itemDef)
        {
            var item = itemsService.CreateItem(itemDef);
            item.View.OnUse += () => UseItem(item).Forget();
        }

        private async UniTask UseItem(Item item)
        {
            item.View.transform.parent = sceneLinks.CenterSocket;
            await item.View.transform.DOLocalMove(Vector3.zero, 0.5f);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            var affect = item.GetAffectGame();
            affectGames.Add(affect);
            if (affect is IAffectEnemySign affectEnemySign)
            {
                enemyPlayer.AddAffects(affectEnemySign);
            }
            itemsService.Release(item);
            
            PrintEnemyState();
        }

        public void OnEnter()
        {
            currentRound = 0;
            var baseDeck = new List<Sign>();
            baseDeck.AddRange(Enumerable.Repeat(Sign.Rock, 2));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Paper, 2));
            baseDeck.AddRange(Enumerable.Repeat(Sign.Scissors, 2));
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
                var card = Object.Instantiate(sceneLinks.CardPrefab);
                sceneLinks.PlayerCardsParent.Add(card);
                card.SetSign(sign);
                
                card.OnClick += () => OnCardSelect(card);
            }
            
            foreach (var sign in enemyPlayer.SignsHand)
            {
                var card = Object.Instantiate(sceneLinks.CardPrefab);
                sceneLinks.EnemyCardsParent.Add(card);
                card.SetSign(sign);
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
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            var winner = GetWinner(enemyPlayer.SelectedSign, cardView.SelectedSign);
            Debug.Log($"Winner: {winner}, Enemy Sign: {enemyPlayer.SelectedSign}, Player Sign: {cardView.SelectedSign}");

            if (affectGames.FirstOrDefault(e => e is IAffectWinner) is IAffectWinner affectWinner)
            {
                affectGames.Remove(affectWinner);
                winner = affectWinner.AffectWinner(winner);
            }
            
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

            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            
            sceneLinks.WinTitle.SetActive(false);
            sceneLinks.LoseTitle.SetActive(false);
            sceneLinks.DrawTitle.SetActive(false);

            sceneLinks.Hands.SetActive(false);
            sceneLinks.GameUI.SetActive(true);

            enemyPlayer.ClearAffects();
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