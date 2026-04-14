using UnityEngine;
using VContainer;

namespace Game.Core.Contexts
{
    public abstract class BaseServiceLocator<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static IObjectResolver resolver;

        public static void Initialize(IObjectResolver newResolver)
        {
            resolver = newResolver;
        }

        public static TType Resolve<TType>() => resolver.Resolve<TType>();
        
        public static TType ResolveOrDefault<TType>() => resolver.ResolveOrDefault<TType>();
        public static bool TryResolve<TType>(out TType result) => resolver.TryResolve(out result);
        
        protected BaseServiceLocator() { }
    }

    public class RootServiceLocator : BaseServiceLocator<RootServiceLocator> { }
    public class EditorServiceLocator : BaseServiceLocator<EditorServiceLocator> { }
    public class GameServiceLocator : BaseServiceLocator<GameServiceLocator> { }

    // Alias for GameServiceLocator

}