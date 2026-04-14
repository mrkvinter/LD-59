namespace Code.Game.Core.ExecutorSystem.Conditions
{
    public static class ConditionExecutorExtensions
    {
        public static bool Execute(this ActionExecutorSystem executor, ConditionJunction junction)
        {
            if (junction.Conditions == null || junction.Conditions.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < junction.Conditions.Length; i++)
            {
                var result = executor.Execute<Condition, bool>(junction.Conditions[i]);
                if (junction.JunctionType == JunctionType.And && !result)
                {
                    return false;
                }

                if (junction.JunctionType == JunctionType.Or && result)
                {
                    return true;
                }
            }
            
            return junction.JunctionType == JunctionType.And;
        }
    }
}