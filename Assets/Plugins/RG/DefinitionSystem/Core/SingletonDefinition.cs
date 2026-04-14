using System;

namespace RG.DefinitionSystem.Core
{
    [Serializable]
    public abstract class SingletonDefinition : Definition
    {
        public override string Id => GetType().Name;
    }
}