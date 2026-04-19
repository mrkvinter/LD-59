using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;

namespace Code.Game.Scripts.Battle.Items
{
    public class Knife : Item
    {
        private CardView selectedCard;

        public override async UniTask OnUse(BattleState battleState)
        {
            View.gameObject.SetActive(false);
            IsSelectable = false;
            var sceneLinks = battleState.SceneLinks;
            sceneLinks.ItemDescription.Hide();
            sceneLinks.ItemHolder.OccupantsList.ForEach(e => e.Item.IsSelectable = false);
            var tcs = new UniTaskCompletionSource();
            battleState.Player.CardsHand.ForEach(card => card.View.IsSelectable = false);
            battleState.EnemyPlayer.CardsHand.ForEach(card =>
            {
                card.View.IsSelectable = true;
                card.View.OnClick += () => OnCardSelect(card.View);
            });
            sceneLinks.VC_LookAtEnemyCard.Priority.Enabled = true;
            sceneLinks.ItemDescription.Show(null, "Select enemy card");
            await tcs.Task;
            sceneLinks.ItemDescription.Hide();
            sceneLinks.VC_LookAtEnemyCard.Priority.Enabled = false;

            battleState.OnRoundEnd += OnRoundEnd;
            return;

            void OnRoundEnd()
            {
                selectedCard?.Card.SetIsBLocked(false);
                battleState.OnRoundEnd -= OnRoundEnd;
            }

            void OnCardSelect(CardView cardView)
            {
                selectedCard = cardView;
                cardView.Card.SetIsBLocked(true, () => tcs.TrySetResult());
                battleState.Player.CardsHand.ForEach(card => card.View.IsSelectable = true);
                sceneLinks.ItemHolder.OccupantsList.ForEach(e => e.Item.IsSelectable = true);
                
                battleState.EnemyPlayer.CardsHand.ForEach(card =>
                {
                    card.View.OnClick = null; 
                    card.View.IsSelectable = false;
                });
                
                if (battleState.EnemyPlayer.SelectedCard == cardView.Card)
                    battleState.EnemyPlayer.SelectSign();
            }
        }
    }
}