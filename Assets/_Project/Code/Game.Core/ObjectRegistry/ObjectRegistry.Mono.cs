using UnityEngine;
using VContainer.Unity;

namespace Code.Game.Core
{
    public partial class ObjectRegistry
    {
        public abstract class MonoRegistered : MonoBehaviour, IInitializable
        {
            protected bool IsInitialized { get; private set; }
            public void Initialize()
            {
                IsInitialized = true;
                OnInitialize();
            }

            protected virtual void OnInitialize() { }
            protected virtual void OnDispose() { }

        }
        public abstract class MonoRegistered<T> : MonoRegistered
            where T : MonoRegistered<T>
        {
            protected void Start()
            {
                instance?.Register(this as T);
            }

            protected void OnDestroy()
            {
                instance?.Unregister(this as T);

                if (IsInitialized)
                {
                    OnDispose();
                }
            }
        }
    }
}