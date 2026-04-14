using Code.Game.Core.Gameplay.ValueTables;
using Game.Core;

namespace Code.Game.Scripts
{
    public class GameContext : IGameContext
    {
        public ValueTable ValueTable { get; } = new();
    }
}