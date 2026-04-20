using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Game.Scripts.Battle.Items
{
    public class Whetstone : Item
    {
        public override bool IsBigItem => true;

        public override async UniTask OnUse(BattleState battleState, Player player)
        {
            await MoveToCenter(battleState.SceneLinks);
            await UniTask.Delay(500);

            var opponent = player == battleState.Player ? battleState.EnemyPlayer : battleState.Player;
            var opponentHolder = player == battleState.Player
                ? battleState.SceneLinks.EnemyCardsParent
                : battleState.SceneLinks.PlayerCardsParent;

            var candidates = opponent.CardsHand.Where(c => c != opponent.SelectedCard).ToList();
            if (candidates.Count > 0)
            {
                var target = candidates[Random.Range(0, candidates.Count)];
                if (target.View != null)
                {
                    opponentHolder.Remove(target.View);
                    Object.Destroy(target.View.gameObject);
                }
                opponent.RemoveCard(target);
            }

            await MoveDown();
        }
    }
}