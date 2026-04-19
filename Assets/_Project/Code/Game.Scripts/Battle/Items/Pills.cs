using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Code.Game.Scripts.Battle.Items
{
    public class Pills : Item
    {
        public override async UniTask OnUse(BattleState battleState)
        {
            await MoveToCenter(battleState.SceneLinks);

            battleState.ScoreForRound = 2;
            battleState.OnRoundEnd += OnRoundEnd;

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
                battleState.ScoreForRound = 0;
                battleState.OnRoundEnd -= OnRoundEnd;
            }
        }
    }

    public class FortuneCookie : Item
    {
        private const string DefaultStateName = "Default";
        private const string ShowingStateName = "Show";
        public override async UniTask OnUse(BattleState battleState)
        {
            await MoveToCenter(battleState.SceneLinks);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            View.Text.text = battleState.EnemyPlayer.SelectedCard.SignDef.Name;
            var cts = new UniTaskCompletionSource();
            View.StatefulObject.SetState(ShowingStateName, true, () => cts.TrySetResult());
            await cts.Task;
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            await MoveDown();
        }
    }
}