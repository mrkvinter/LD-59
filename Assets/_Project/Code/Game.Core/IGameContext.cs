using Code.Game.Core.Gameplay.ValueTables;

namespace Game.Core
{
    public interface IGameContext
    {
        ValueTable ValueTable { get; }
    }
}