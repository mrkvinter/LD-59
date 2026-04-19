using System;
using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class SpareSignalFlare : Item
    {
        public override async UniTask OnUse(BattleState battleState)
        {
            await MoveToCenter(battleState.SceneLinks);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            battleState.Player.AddHealth(1);
            battleState.UpdateSignalFlares();
            await MoveDown();
        }
    }
}