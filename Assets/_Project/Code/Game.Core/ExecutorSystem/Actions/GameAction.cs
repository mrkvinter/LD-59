using System;

namespace Code.Game.Core.ExecutorSystem.Actions
{
    public abstract class GameAction : BaseExecutorVoid
    {
        public override string Id => GetType().Name;
    }
    
    [Serializable]
    public struct GameActionJunction
    {
        public ExecutorInfo<GameAction>[] Actions;
    }
}