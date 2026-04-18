namespace Code.Game.Scripts.Battle.Items
{
    public class Item
    {
        public ItemView View { get; private set; }
        public ItemDef ItemDef { get; set; }
        public virtual IAffectGame GetAffectGame() => null;
        public string Name => ItemDef.Name;
        public string Description => ItemDef.Description;

        public void SetView(ItemView view)
        {
            View = view;
            View.Item = this;
        }
    }

    public interface IAffectGame
    {
    }
    
    public interface IAffectWinner : IAffectGame
    {
        public virtual Winner AffectWinner(Winner winner) => winner;
    }
}