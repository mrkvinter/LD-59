using System;
using System.Collections.Generic;
using System.Linq;
using Code.Game.Core;
using Code.Game.Scripts.Battle.Items;
using Cysharp.Threading.Tasks;
using RG.DefinitionSystem.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Scripts.Battle
{
    public class BattleState
    {
        private int currentRound;
        public Player EnemyPlayer;
        public Player Player;
        public readonly SceneLinks SceneLinks = G.Resolve<SceneLinks>();

        private readonly ItemsService itemsService;

        private List<IAffectGame> affectGames = new();
        private List<DefRef<ItemDef>> items = new();
        private Queue<DefRef<ItemDef>> itemsQueue = new();

        public Action OnTurnEnd;
        public event Action OnRoundEnd;
        public event Action<bool> OnGameEnd;

        public int ScoreForScissors = 1;
        public int ScoreForRock = 1;
        public int ScoreForPaper = 1;

        public int ItemsPerRound;
        public int CardsPerRound = 5;
        public int WinStones = 3;

        public bool NotStartNextRound = false;
        public bool IsPickingCard;

        public BattleState()
        {
            itemsService = new ItemsService();
        }

        public void AddAfffect(IAffectGame affectGame) => affectGames.Add(affectGame);

        private void AddItem(DefRef<ItemDef> itemDef)
        {
            var item = itemsService.CreateItem(itemDef);
            if (item == null) return;

            item.IsSelectable = true;
            Player.Items.Add(item);
            item.View.OnUse += () => UseItem(item, Player).Forget();
        }

        private void RefillItems()
        {
            itemsQueue = new Queue<DefRef<ItemDef>>(items.OrderBy(_ => Guid.NewGuid()));
        }

        private void AddItems(int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (itemsQueue.Count == 0) RefillItems();

                AddItem(itemsQueue.Dequeue());
            }
        }

        private async UniTask UseItem(Item item, Player player)
        {
            player.Items.Remove(item);
            item.IsSelectable = false;
            await item.OnUse(this, player);
            itemsService.Release(item);
        }

        public void StartBattle(Player player, Player enemy, List<DefRef<ItemDef>> items)
        {
            currentRound = 0;

            itemsQueue = new Queue<DefRef<ItemDef>>(items.OrderBy(_ => Guid.NewGuid()));
            this.items.AddRange(items);

            EnemyPlayer = enemy;
            Player = player;

            // AddItem(ItemDefType.BrokenGlass);
            // AddItem(ItemDefType.Knife);
            AddItem(ItemDefType.ToiletPaper);
            AddItem(ItemDefType.Pills);
            // AddItem(ItemDefType.SpareSignalFlare);
            // AddItem(ItemDefType.Whetstone);
            // AddItem(ItemDefType.FortuneCookie);

            SceneLinks.EnemyHealthPanel.SetHealthCount(Player.Health);
            SceneLinks.PlayerHealthPanel.SetHealthCount(EnemyPlayer.Health);

            StartNewRound();
        }

        public void OnExit()
        {
            EnemyPlayer.ClearHand(SceneLinks.EnemyCardsParent);
            Player.ClearHand(SceneLinks.PlayerCardsParent);
            
            foreach (var playerItem in Player.Items)
            {
                SceneLinks.ItemHolder.Release(playerItem.View);
                Object.Destroy(playerItem.View.gameObject);
            }
            
            Player.Items.Clear();

            foreach (var item in EnemyPlayer.Items)
            {
                SceneLinks.EnemyItemHolder.Release(item.View);
                Object.Destroy(item.View.gameObject);
            }
            
            EnemyPlayer.Items.Clear();
        }

        private void NextTurn()
        {
            if (EnemyPlayer.CardsHand.Count == 0 || Player.CardsHand.Count == 0 ||
                EnemyPlayer.WinStones >= WinStones || Player.WinStones >= WinStones)
            {
                OnRoundEnd?.Invoke();

                if (NotStartNextRound) return;

                StartNewRound();
            }

            Player.Items.ForEach(e => e.BlockedForRound = false);
            EnemyPlayer.Items.ForEach(e => e.BlockedForRound = false);

            EnemyPlayer.SelectSign();
        }

        public void StartNewRound()
        {
            currentRound++;
            AddItems(ItemsPerRound);

            if (Player.WinStones > EnemyPlayer.WinStones)
            {
                EnemyPlayer.ReduceHealth(1);
            }

            if (EnemyPlayer.WinStones > Player.WinStones)
            {
                Player.ReduceHealth(1);
            }

            if (Player.Health <= 0)
            {
                OnGameEnd?.Invoke(false);
                return;
            }

            if (EnemyPlayer.Health <= 0)
            {
                OnGameEnd?.Invoke(true);
                return;
            }


            Player.WinStones = 0;
            EnemyPlayer.WinStones = 0;
            UpdateAll();

            EnemyPlayer.ClearHand(SceneLinks.EnemyCardsParent);
            Player.ClearHand(SceneLinks.PlayerCardsParent);
            EnemyPlayer.RefillBag();
            Player.RefillBag();
            EnemyPlayer.Draw(CardsPerRound);
            Player.Draw(CardsPerRound);
            
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
            if (IsPickingCard) return;
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

            await UniTask.Delay(TimeSpan.FromSeconds(.5f));

            var (winner, winnerSign) = GetWinner(EnemyPlayer.SelectedCard?.Sign ?? Sign.None, cardView.SelectedSign);
            Debug.Log(
                $"Winner: {winner}, Enemy Sign: {EnemyPlayer.SelectedCard.Sign}, Player Sign: {cardView.SelectedSign}");

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

            if (winner == Winner.Right)
            {
                SceneLinks.WinTitle.SetActive(true);
                Player.WinStones += scoreForRound * Player.ScoreMultiplayer;
                // EnemyPlayer.ReduceHealth(scoreForRound);
            }

            if (winner == Winner.Left)
            {
                SceneLinks.LoseTitle.SetActive(true);
                EnemyPlayer.WinStones += scoreForRound * EnemyPlayer.ScoreMultiplayer;
                // Player.ReduceHealth(scoreForRound);
            }

            if (winner == Winner.Draw) SceneLinks.DrawTitle.SetActive(true);

            Player.RemoveCard(cardView.Card);

            UpdateAll();
            var enemySelectedCard = EnemyPlayer.SelectedCard;
            SceneLinks.EnemyCardsParent.Remove(enemySelectedCard.View);
            EnemyPlayer.RemoveSelectedSign();
            Object.Destroy(enemySelectedCard.View.gameObject);

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            SceneLinks.WinTitle.SetActive(false);
            SceneLinks.LoseTitle.SetActive(false);
            SceneLinks.DrawTitle.SetActive(false);

            SceneLinks.Hands.SetActive(false);
            SceneLinks.GameUI.SetActive(true);

            EnemyPlayer.ClearAffects();
            EnemyPlayer.SelectSign();

            if (Player.Health <= 0)
            {
                OnGameEnd?.Invoke(false);
                return;
            }

            if (EnemyPlayer.Health <= 0)
            {
                OnGameEnd?.Invoke(true);
                return;
            }

            OnTurnEnd?.Invoke();

            NextTurn();
        });

        public async UniTask<CardView> PickCardAsync()
        {
            IsPickingCard = true;

            var tcs = new UniTaskCompletionSource<CardView>();
            var unsubs = new List<Action>();

            foreach (var card in Player.CardsHand.Concat(EnemyPlayer.CardsHand))
            {
                var view = card.View;
                if (view == null) continue;

                Action handler = () => tcs.TrySetResult(view);
                view.OnClick += handler;
                unsubs.Add(() => view.OnClick -= handler);
            }

            try
            {
                return await tcs.Task;
            }
            finally
            {
                foreach (var unsub in unsubs) unsub();
                IsPickingCard = false;
            }
        }

        public void DuplicateCardForPlayer(Card source)
        {
            if (source == null) return;

            var clone = new Card(source.SignDef);
            Player.CardsHand.Add(clone);

            var cardView = Object.Instantiate(SceneLinks.CardPrefab);
            SceneLinks.PlayerCardsParent.Add(cardView);
            clone.SetView(cardView);

            cardView.OnClick += () => OnCardSelect(cardView);
        }

        public void UpdateAll()
        {
            SceneLinks.EnemyHealthPanel.SetHealthCount(EnemyPlayer.Health);
            SceneLinks.PlayerHealthPanel.SetHealthCount(Player.Health);
            SceneLinks.PlayerWinStones.SetHealthCount(Player.WinStones);
            SceneLinks.EnemyWinStones.SetHealthCount(EnemyPlayer.WinStones);
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