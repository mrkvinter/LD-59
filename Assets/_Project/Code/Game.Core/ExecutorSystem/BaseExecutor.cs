using Game.Core;

namespace Code.Game.Core.ExecutorSystem
{
    public abstract class BaseExecutor
    {
        public abstract string Id { get; }
    }

    public abstract class BaseExecutor<T> : BaseExecutor
    {
        public abstract T Execute(IGameContext context);
    }
    
    public abstract class BaseExecutorVoid : BaseExecutor
    {
        public abstract void Execute(IGameContext context);
    }
}