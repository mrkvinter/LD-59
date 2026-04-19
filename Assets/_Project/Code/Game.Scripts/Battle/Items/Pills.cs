using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Code.Game.Scripts.Battle.Items
{
    public class Pills : Item
    {
        public override async UniTask OnUse(BattleState battleState)
        {
            BlockSameItemsWithinRound(battleState, battleState.Player);
            await MoveToCenter(battleState.SceneLinks);

            battleState.ScoreMultiplayer *= 2;
            battleState.OnTurnEnd += OnRoundEnd;

            await DOTween.To(
                    () => battleState.SceneLinks.PillsVolume.weight,
                    x => battleState.SceneLinks.PillsVolume.weight = x, 1, 1f)
                .SetEase(Ease.OutSine);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25));
            await DOTween.To(
                    () => battleState.SceneLinks.PillsVolume.weight,
                    x => battleState.SceneLinks.PillsVolume.weight = x, 0.25f, .5f)
                .SetEase(Ease.InSine);

            await MoveDown();


            void OnRoundEnd()
            {
                DOTween.To(
                    () => battleState.SceneLinks.PillsVolume.weight,
                    x => battleState.SceneLinks.PillsVolume.weight = x, 0, .5f);
                battleState.ScoreMultiplayer = 1;
                battleState.OnTurnEnd -= OnRoundEnd;
            }
        }
    }
}