using Cysharp.Threading.Tasks;

namespace Code.Game.Scripts.Battle.Items
{
    public abstract class Item
    {
        public ItemView View { get; private set; }
        public ItemDef ItemDef { get; set; }
        public virtual IAffectGame GetAffectGame() => null;
        public string Name => ItemDef.Name;
        public string Description => ItemDef.Description;
        public bool IsSelectable { get; set; }

        public void SetView(ItemView view)
        {
            View = view;
            if (View != null) View.Item = this;
        }
        
        public virtual UniTask OnUse(BattleState battleState) => UniTask.CompletedTask;
    }

    public interface IAffectGame
    {
    }
    
    public interface IAffectWinner : IAffectGame
    {
        public virtual Winner AffectWinner(Winner winner) => winner;
    }
    
    public interface IAffectEnemySign : IAffectGame
    {
        public bool IsSignAvailable(Card card);
    }
}