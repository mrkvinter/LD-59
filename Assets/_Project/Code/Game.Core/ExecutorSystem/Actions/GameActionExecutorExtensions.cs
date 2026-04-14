namespace Code.Game.Core.ExecutorSystem.Actions
{
    public static class GameActionExecutorExtensions
    {
        public static void ExecuteGameActionJunction(this ActionExecutorSystem executor, GameActionJunction junction)
        {
            for (var i = 0; i < junction.Actions.Length; i++)
            {
                executor.Execute(junction.Actions[i]);
            }
        }
    }
}