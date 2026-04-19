using System;
using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class BrokenGlasses : Item
    {
        public override IAffectGame GetAffectGame() => new BrokenGlassesAffect();

        public override UniTask OnUse(BattleState battleState)
        {
            battleState.AddAfffect(new BrokenGlassesAffect());
            
            return UniTask.CompletedTask;
        }

        private class BrokenGlassesAffect : IAffectWinner
        {
            public Winner AffectWinner(Winner winner) => winner switch
            {
                Winner.Left => Winner.Right,
                Winner.Right => Winner.Left,
                _ => winner
            };
        }

    }
}