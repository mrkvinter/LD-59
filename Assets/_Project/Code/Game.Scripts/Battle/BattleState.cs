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

        public int ScoreForRound = 1;

        public BattleState()
        {
            itemsService = new ItemsService();

            AddItem(ItemDefType.BrokenGlass);
            AddItem(ItemDefType.Knife);
            AddItem(ItemDefType.Pills);
            AddItem(ItemDefType.FortuneCookie);
        }

        public void AddAfffect(IAffectGame affectGame) => affectGames.Add(affectGame);
        private void AddItem(DefRef<ItemDef> itemDef)
        {
            var item = itemsService.CreateItem(itemDef);
            item.IsSelectable = true;
            item.View.OnUse += () => UseItem(item).Forget();
        }

        private async UniTask UseItem(Item item)
        {
            // item.View.transform.parent = SceneLinks.CenterSocket;
            // item.View.transform.DOLocalRotate(Vector3.zero, 0.25f);
            // await item.View.transform.DOLocalMove(Vector3.zero, 0.25f);
            // await UniTask.Delay(TimeSpan.FromSeconds(.5f));
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
            
            var winner = GetWinner(EnemyPlayer.SelectedCard?.Sign ?? Sign.None, cardView.SelectedSign);
            Debug.Log($"Winner: {winner}, Enemy Sign: {EnemyPlayer.SelectedCard.Sign}, Player Sign: {cardView.SelectedSign}");

            if (affectGames.FirstOrDefault(e => e is IAffectWinner) is IAffectWinner affectWinner)
            {
                affectGames.Remove(affectWinner);
                winner = affectWinner.AffectWinner(winner);
            }
            
            if (winner == Winner.Right)
            {
                SceneLinks.WinTitle.SetActive(true);
                EnemyPlayer.ReduceHealth(ScoreForRound);
            }
            if (winner == Winner.Left)
            {
                SceneLinks.LoseTitle.SetActive(true);
                Player.ReduceHealth(ScoreForRound);
            }
            if (winner == Winner.Draw) SceneLinks.DrawTitle.SetActive(true);
            
            Player.RemoveCard(cardView.Card);

            SceneLinks.EnemyHealthPanel.SetHealthCount(EnemyPlayer.Health);
            SceneLinks.PlayerHealthPanel.SetHealthCount(Player.Health);
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

        private Winner GetWinner(Sign leftSign, Sign rightSign)
        {
            if (leftSign == Sign.None && rightSign == Sign.None) return Winner.Draw;
            if (leftSign == Sign.None) return Winner.Right;
            if (rightSign == Sign.None) return Winner.Left;

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