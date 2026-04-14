using System;

namespace Code.Game.Core.ExecutorSystem
{
    [Serializable]
    public struct ExecutorInfo<T>
        where T : BaseExecutor
    {
        public Type Type => typeof(T);

        public bool IsNull => Id == "None";

        public string Id;
        public string Args;
    }
}