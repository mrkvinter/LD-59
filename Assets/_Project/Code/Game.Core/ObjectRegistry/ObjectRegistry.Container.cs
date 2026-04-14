using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Code.Game.Core
{
    public partial class ObjectRegistry
    {
        private abstract class Container
        {
            public abstract IReadOnlyCollection<MonoRegistered> BaseObjects { get; }
        }

        private class Container<T> : Container
            where T : MonoRegistered
        {
            private readonly List<T> objects = new();

            public IReadOnlyCollection<T> Objects { get; }

            public override IReadOnlyCollection<MonoRegistered> BaseObjects => objects;

            public Container()
            {
                Objects = new ReadOnlyCollection<T>(objects);
            }

            public void Add(T item)
            {
                objects.Add(item);
            }

            public bool Contains(T item)
            {
                return objects.Contains(item);
            }

            public bool Remove(T item)
            {
                return objects.Remove(item);
            }
        }
    }
}