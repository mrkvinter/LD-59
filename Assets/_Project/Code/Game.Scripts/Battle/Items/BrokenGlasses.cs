namespace Code.Game.Scripts.Battle.Items
{
    public class BrokenGlasses : Item
    {
        public override IAffectGame GetAffectGame() => new BrokenGlassesAffect();

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
    
    public class Knife : Item
    {
        public override IAffectGame GetAffectGame() => new KnifeAffect();

        private class KnifeAffect : IAffectEnemySign
        {
            public bool IsSignAvailable(Sign sign) => sign != Sign.Fuck;
        }
    }
}