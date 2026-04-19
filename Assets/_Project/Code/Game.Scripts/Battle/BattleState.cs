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
        public Player EnemyPlayer;
        public Player Player;
        public readonly SceneLinks SceneLinks = G.Resolve<SceneLinks>();

        private readonly ItemsService itemsService;
        
        private List<IAffectGame> affectGames = new();
        
        public Action OnRoundEnd;

        public int ScoreMultiplayer = 1;
        public int ScoreForScissors = 1;
        public int ScoreForRock = 1;
        public int ScoreForPaper = 1;
        

        public BattleState()
        {
            itemsService = new ItemsService();
        }

        public void AddAfffect(IAffectGame affectGame) => affectGames.Add(affectGame);
        private void AddItem(DefRef<ItemDef> itemDef)
        {
            var item = itemsService.CreateItem(itemDef);
            item.IsSelectable = true;
            Player.Items.Add(item);
            item.View.OnUse += () => UseItem(item, Player).Forget();
        }

        private async UniTask UseItem(Item item, Player player)
        {
            player.Items.Remove(item);
            item.IsSelectable = false;
            await item.OnUse(this);
            itemsService.Release(item);
        }

        public void OnEnter()
        {
            currentRound = 0;
            var baseDeck = new List<DefRef<SignDef>>();
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Rock, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Papper, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Scissors, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Goat, 1));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.F, 1));
            
            EnemyPlayer = new Player(3, baseDeck.OrderBy(_ => Guid.NewGuid()).ToList());
            Player = new Player(3, baseDeck.OrderBy(_ => Guid.NewGuid()).ToList());
            
            // AddItem(ItemDefType.BrokenGlass);
            AddItem(ItemDefType.Knife);
            AddItem(ItemDefType.Pills);
            AddItem(ItemDefType.Pills);
            AddItem(ItemDefType.SpareSignalFlare);
            AddItem(ItemDefType.Whetstone);
            AddItem(ItemDefType.FortuneCookie);
            
            SceneLinks.EnemyHealthPanel.SetHealthCount(Player.Health);
            SceneLinks.PlayerHealthPanel.SetHealthCount(EnemyPlayer.Health);
            
            StartRound();
        }

        public void OnExit()
        {
            
        }

        private void StartRound()
        {
            currentRound++;
            EnemyPlayer.Draw(SignsPerRound);
            Player.Draw(SignsPerRound);
            
            EnemyPlayer.SelectSign();
            
            InstantiateCards();
        }

        private void InstantiateCards()
        {
            foreach (var card in Player.CardsHand)
            {
                var cardView = Object.Instantiate(SceneLinks.CardPrefab);
                SceneLinks.PlayerCardsParent.Add(cardView);
                card.SetView(cardView);
                
                cardView.OnClick += () => OnCardSelect(cardView);
            }
            
            foreach (var card in EnemyPlayer.CardsHand)
            {
                var cardView = Object.Instantiate(SceneLinks.CardPrefab);
                SceneLinks.EnemyCardsParent.Add(cardView);
                card.SetView(cardView);
            }
        }

        private void OnCardSelect(CardView cardView) => UniTask.Create(async () =>
        {
            Object.Destroy(cardView.gameObject);
            SceneLinks.Hands.SetActive(true);
            SceneLinks.GameUI.SetActive(false);

            SceneLinks.LeftHandView.SetVisible(true);
            SceneLinks.RightHandView.SetVisible(true);

            if (EnemyPlayer.SelectedCard == null) SceneLinks.LeftHandView.SetVisible(false);
            
            SceneLinks.LeftHandView.SetSign(Sign.Rock);
            SceneLinks.RightHandView.SetSign(Sign.Rock);

            SceneLinks.PlayerCardsParent.Remove(cardView);

            SceneLinks.LeftHandView.PlayShakeAnimation().Forget();
            await SceneLinks.RightHandView.PlayShakeAnimation();
            
            SceneLinks.LeftHandView.SetSign(EnemyPlayer.SelectedCard.Sign);
            SceneLinks.RightHandView.SetSign(cardView.SelectedSign);
            
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            
            var (winner, winnerSign) = GetWinner(EnemyPlayer.SelectedCard?.Sign ?? Sign.None, cardView.SelectedSign);
            Debug.Log($"Winner: {winner}, Enemy Sign: {EnemyPlayer.SelectedCard.Sign}, Player Sign: {cardView.SelectedSign}");

            if (affectGames.FirstOrDefault(e => e is IAffectWinner) is IAffectWinner affectWinner)
            {
                affectGames.Remove(affectWinner);
                winner = affectWinner.AffectWinner(winner);
            }
            
            var scoreForRound = winnerSign switch
            {
                Sign.Rock => ScoreForRock,
                Sign.Paper => ScoreForPaper,
                Sign.Scissors => ScoreForScissors,
                _ => 1
            };
            
            scoreForRound *= ScoreMultiplayer;
            
            if (winner == Winner.Right)
            {
                SceneLinks.WinTitle.SetActive(true);
                EnemyPlayer.ReduceHealth(scoreForRound);
            }
            if (winner == Winner.Left)
            {
                SceneLinks.LoseTitle.SetActive(true);
                Player.ReduceHealth(scoreForRound);
            }
            if (winner == Winner.Draw) SceneLinks.DrawTitle.SetActive(true);
            
            Player.RemoveCard(cardView.Card);

            UpdateSignalFlares();
            var enemySelectedCard = EnemyPlayer.SelectedCard;
            SceneLinks.EnemyCardsParent.Remove(enemySelectedCard.View);
            EnemyPlayer.RemoveSelectedSign();
            Object.Destroy(enemySelectedCard.View.gameObject);

            await UniTask.Delay(TimeSpan.FromSeconds(2f));
            
            SceneLinks.WinTitle.SetActive(false);
            SceneLinks.LoseTitle.SetActive(false);
            SceneLinks.DrawTitle.SetActive(false);

            SceneLinks.Hands.SetActive(false);
            SceneLinks.GameUI.SetActive(true);

            EnemyPlayer.ClearAffects();
            EnemyPlayer.SelectSign();
            
            OnRoundEnd?.Invoke();
        });

        public void UpdateSignalFlares()
        {
            SceneLinks.EnemyHealthPanel.SetHealthCount(EnemyPlayer.Health);
            SceneLinks.PlayerHealthPanel.SetHealthCount(Player.Health);
        }

        private (Winner, Sign) GetWinner(Sign leftSign, Sign rightSign)
        {
            if (leftSign == Sign.None && rightSign == Sign.None) return (Winner.Draw, Sign.None);
            if (leftSign == Sign.None) return (Winner.Right, rightSign);
            if (rightSign == Sign.None) return (Winner.Left, leftSign);

            var leftSignDef = SignDef.Get(leftSign);
            var rightSignDef = SignDef.Get(rightSign);
            var isLeftWin = leftSignDef.BeatSigns.Contains(rightSignDef.Sign);
            var isRightWin = rightSignDef.BeatSigns.Contains(leftSignDef.Sign);
            
            return (isLeftWin, isRightWin) switch
            {
                (true, true) => (Winner.Draw, Sign.None),
                (true, false) => (Winner.Left, leftSign),
                (false, true) => (Winner.Right, rightSign),
                _ => (Winner.Draw, Sign.None)
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