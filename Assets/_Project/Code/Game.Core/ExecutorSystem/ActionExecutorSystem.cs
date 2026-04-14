using System.Collections.Generic;
using Code.Game.Core.ExecutorSystem.Actions;
using Game.Core;
using UnityEngine;

namespace Code.Game.Core.ExecutorSystem
{
    public class ActionExecutorSystem
    {
        private readonly IGameContext context;
        private readonly Dictionary<string, BaseExecutor> executors = new();

        public ActionExecutorSystem(IEnumerable<BaseExecutor> executors, IGameContext context)
        {
            this.context = context;
            foreach (var executor in executors)
            {
                this.executors.Add(executor.Id, executor);
            }
        }

        public T GetExecutor<T>(string id) where T : BaseExecutor
        {
            if (executors.TryGetValue(id, out var executor))
            {
                return (T)executor;
            }

            return null;
        }
        
        public List<T> GetExecutors<T>() where T : BaseExecutor
        {
            var result = new List<T>();
            foreach (var executor in executors.Values)
            {
                if (executor is T t)
                {
                    result.Add(t);
                }
            }

            return result;
        }

        public TResult Execute<TExecutor, TResult>(ExecutorInfo<TExecutor> executorInfo)
            where TExecutor : BaseExecutor<TResult>
        {
            if (executorInfo.Id != null && executors.TryGetValue(executorInfo.Id, out var executor))
            {
                var e = (TExecutor)executor;
                JsonUtility.FromJsonOverwrite(executorInfo.Args, e);
                var result = e.Execute(context);

                return result;
            }

            return default;
        }
        
        public void Execute<TExecutor>(ExecutorInfo<TExecutor> executorInfo)
            where TExecutor : BaseExecutorVoid
        {
            if (!string.IsNullOrEmpty(executorInfo.Id) && executors.TryGetValue(executorInfo.Id, out var executor))
            {
                var e = (TExecutor)executor;
                JsonUtility.FromJsonOverwrite(executorInfo.Args, e);
                e.Execute(context);
            }
        }
        
        public void Execute(GameActionJunction actionJunction)
        {
            if (actionJunction.Actions == null)
            {
                return;
            }

            foreach (var action in actionJunction.Actions)
            {
                Execute(action);
            }
        }
    }
}