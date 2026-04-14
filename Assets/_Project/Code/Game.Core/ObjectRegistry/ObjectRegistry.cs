using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Game.Core
{
    public partial class ObjectRegistry
    {
        private static ObjectRegistry instance;

        private readonly Dictionary<Type, Container> containers = new();

        public event Action<MonoRegistered> OnRegistered;
        public event Action<MonoRegistered> OnUnregistered;

        public IEnumerable<MonoRegistered> Enumerate()
        {
            foreach (var container in containers.Values.ToList())
            {
                foreach (var obj in container.BaseObjects)
                {
                    yield return obj;
                }
            }
        }

        public void Initialize()
        {
            if (instance != null)
                throw new Exception("ObjectRegistry already initialized");

            instance = this;
        }

        public IReadOnlyCollection<T> Get<T>() where T : MonoRegistered
        {
            return GetContainer<T>().Objects;
        }

        public IEnumerable<T> Enumerate<T>() where T : MonoRegistered
        {
            foreach (var (type, container) in containers)
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    foreach (var obj in container.BaseObjects)
                    {
                        yield return (T)obj;
                    }
                }
            }
        }

        public void Register<T>(T obj)
            where T : MonoRegistered
        {
            var container = GetContainer<T>();
            if (container.Contains(obj))
            {
                Debug.LogError($"Object {obj.name} already registered");
                return;
            }

            container.Add(obj);

            OnRegistered?.Invoke(obj);
        }

        public void Unregister<T>(T obj)
            where T : MonoRegistered
        {
            var container = GetContainer<T>();
            container.Remove(obj);

            OnUnregistered?.Invoke(obj);
        }

        private Container<T> GetContainer<T>()
            where T : MonoRegistered
        {
            var type = typeof(T);
            containers.TryAdd(type, new Container<T>());

            return containers[type] as Container<T>;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void Reload()
        {
            instance = null;
        }
    }
}