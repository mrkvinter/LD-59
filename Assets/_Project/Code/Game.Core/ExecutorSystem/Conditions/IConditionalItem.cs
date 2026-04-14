namespace Code.Game.Core.ExecutorSystem.Conditions
{
    public interface IConditionalItem
    {
        ConditionJunction ConditionJunction { get; }
        bool IsActive { get; }
        void Activate();
        void Deactivate();
    }
}