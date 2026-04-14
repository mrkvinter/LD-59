using System;
using Game.Core;
using Sirenix.Utilities;

namespace Code.Game.Core.ExecutorSystem.Conditions
{
    public abstract class Condition : BaseExecutor<bool>
    {
        public event Action OnConditionChanged;

        public override string Id => GetType().GetNiceName();

        public virtual string Description => Id;

        public abstract void OnStartTracking(IGameContext gameContext);
        public abstract void OnDisposeTracking(IGameContext gameContext);

        protected void RaiseConditionChanged()
        {
            OnConditionChanged?.Invoke();
        }
    }

    [Serializable]
    public struct ConditionJunction
    {
        public JunctionType JunctionType;

        public ExecutorInfo<Condition>[] Conditions;
    }

    public enum JunctionType
    {
        And,
        Or
    }
    
    public enum ComparisonType
    {
        None,

        Equals,
        MoreThan,
        LessThan,

        LessOrEqual,
        MoreOrEqual
    }
}