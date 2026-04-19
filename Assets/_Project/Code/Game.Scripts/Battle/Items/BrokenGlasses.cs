using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public class BrokenGlasses : Item
    {
        public override IAffectGame GetAffectGame() => new BrokenGlassesAffect();

        public override async UniTask OnUse(BattleState battleState, Player player)
        {
            battleState.AddAfffect(new BrokenGlassesAffect());
            await MoveDown();
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