using System.Collections.Generic;
using Game.Core;

namespace Code.Game.Core.ExecutorSystem.Conditions
{
    public class ConditionService
    {
        private readonly Dictionary<string, ConditionTracker> trackers = new();

        public ConditionService(ActionExecutorSystem executor, IGameContext context)
        {
            var conditionExecutors = executor.GetExecutors<Condition>();

            foreach (var conditionExecutor in conditionExecutors)
            {
                trackers.Add(conditionExecutor.Id, new ConditionTracker(conditionExecutor, executor, context));
            }
        }

        public void Subscribe(IConditionalItem conditionalItem)
        {
            foreach (var conditionInfo in conditionalItem.ConditionJunction.Conditions)
            {
                var tracker = trackers[conditionInfo.Id];
                tracker.Add(conditionalItem);
            }
        }
        
        public void Unsubscribe(IConditionalItem conditionalItem)
        {
            foreach (var conditionInfo in conditionalItem.ConditionJunction.Conditions)
            {
                var tracker = trackers[conditionInfo.Id];
                tracker.Remove(conditionalItem);
            }
        }
        
        private class ConditionTracker
        {
            private readonly Condition condition;
            private readonly ActionExecutorSystem executor;
            private readonly IGameContext context;
            private readonly HashSet<IConditionalItem> items = new();
            
            public ConditionTracker(Condition condition, ActionExecutorSystem executor, IGameContext context)
            {
                this.condition = condition;
                this.executor = executor;
                this.context = context;
            }
            
            public void Add(IConditionalItem conditionalItem)
            {
                if (items.Count == 0)
                {
                    condition.OnStartTracking(context);
                }

                if (items.Add(conditionalItem))
                {
                    condition.OnConditionChanged += OnConditionChanged;
                }
                
                OnConditionChanged(conditionalItem);
            }
            
            public void Remove(IConditionalItem conditionalItem)
            {
                if (items.Remove(conditionalItem))
                {
                    condition.OnConditionChanged -= OnConditionChanged;
                }

                if (items.Count == 0)
                {
                    condition.OnDisposeTracking(context);
                }
            }

            private void OnConditionChanged()
            {
                foreach (var item in items)
                {
                    OnConditionChanged(item);
                }
            }
            
            private void OnConditionChanged(IConditionalItem conditionalItem)
            {
                var conditionResult = executor.Execute(conditionalItem.ConditionJunction);
                switch (conditionResult)
                {
                    case true when !conditionalItem.IsActive:
                        conditionalItem.Activate();
                        break;
                    case false when conditionalItem.IsActive:
                        conditionalItem.Deactivate();
                        break;
                }
            }
        }
    }
}