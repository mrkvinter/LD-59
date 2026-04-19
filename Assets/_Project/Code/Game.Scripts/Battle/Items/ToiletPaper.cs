using System;
using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class ToiletPaper : Item
    {
        public override bool IsBigItem => true;

        public override async UniTask OnUse(BattleState battleState)
        {
            BlockSameItemsWithinRound(battleState, battleState.Player);
            await MoveToCenter(battleState.SceneLinks);

            battleState.ScoreForPaper *= 2;
            battleState.OnTurnEnd += OnRoundEnd;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            await MoveDown();


            void OnRoundEnd()
            {
                battleState.ScoreForScissors = 1;
                battleState.OnTurnEnd -= OnRoundEnd;
            }
        }
    }
}