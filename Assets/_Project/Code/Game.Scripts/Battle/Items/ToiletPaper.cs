using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class ToiletPaper : Item
    {
        public override bool IsBigItem => true;

        public override async UniTask OnUse(BattleState battleState, Player player)
        {
            await MoveToCenter(battleState.SceneLinks);
            await UniTask.Delay(500);
            MoveDown().Forget();
            
            battleState.Player.Items.ForEach(e => e.IsSelectable = false);
            battleState.SceneLinks.ItemDescription.Show("", "Select a card to duplicate");

            var pickedView = await battleState.PickCardAsync();
            if (pickedView != null && pickedView.Card != null)
            {
                battleState.DuplicateCardForPlayer(pickedView.Card);
            }

            battleState.Player.Items.ForEach(e => e.IsSelectable = true);
            battleState.SceneLinks.ItemDescription.Hide();
        }
    }
}