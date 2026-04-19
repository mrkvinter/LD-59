using System;
using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class Whetstone : Item
    {
        public override async UniTask OnUse(BattleState battleState)
        {
            BlockSameItemsWithinRound(battleState, battleState.Player);
            await MoveToCenter(battleState.SceneLinks);

            battleState.ScoreForScissors *= 2;
            battleState.OnRoundEnd += OnRoundEnd;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            await MoveDown();


            void OnRoundEnd()
            {
                battleState.ScoreForScissors = 1;
                battleState.OnRoundEnd -= OnRoundEnd;
            }
        }
    }
}