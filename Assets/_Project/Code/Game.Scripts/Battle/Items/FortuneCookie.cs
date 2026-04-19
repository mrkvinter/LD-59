using System;
using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class FortuneCookie : Item
    {
        private const string DefaultStateName = "Default";
        private const string ShowingStateName = "Show";

        public override async UniTask OnUse(BattleState battleState, Player player)
        {
            await MoveToCenter(battleState.SceneLinks);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            View.Text.text = battleState.EnemyPlayer.SelectedCard == null ? 
                "Praise be the mighty ooze!" : 
                battleState.EnemyPlayer.SelectedCard.SignDef.Name;

            var cts = new UniTaskCompletionSource();
            View.StatefulObject.SetState(ShowingStateName, true, () => cts.TrySetResult());
            await cts.Task;
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            await MoveDown();
        }
    }
}