using Game.Core.Contexts;
using UnityEngine;

namespace Code.Cheats
{
    public interface ICheatCategory
    {
        void Draw();
    }
    
    public abstract class BaseEditorCheatCategory : ICheatCategory
    {
        protected static T Resolve<T>()
        {
#if UNITY_EDITOR
            return Application.isPlaying ? GameServiceLocator.Resolve<T>() : EditorServiceLocator.Resolve<T>();
#else
            return GameServiceLocator.Resolve<T>();
#endif
        }

        public abstract void Draw();
    }
}