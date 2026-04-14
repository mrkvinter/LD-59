using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Scripts.EntitySystem
{
    public class Entity
    {
        private readonly GameObject gameObject;
        private readonly List<IEntityComponent> components = new();

        public Transform Transform => gameObject.transform;

        public virtual Vector3 Forward => Transform.forward;

        public Entity(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        public T AddComponent<T>(T component) where T : IEntityComponent
        {
            components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : IEntityComponent
        {
            foreach (var component in components)
            {
                if (component is T result)
                {
                    return result;
                }
            }

            return default;
        }

        public IEnumerable<T> GetComponents<T>()
        {
            foreach (var component in components)
            {
                if (component is T result)
                {
                    yield return result;
                }
            }
        }
        
        public bool TryGetComponent<T>(out T result) where T : IEntityComponent
        {
            foreach (var component in components)
            {
                if (component is T componentResult)
                {
                    result = componentResult;
                    return true;
                }
            }

            result = default;
            return false;
        }
    }

    public class EntityService
    {
        private List<Entity> entities = new();

        public IReadOnlyList<Entity> Entities => entities;

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
        }
    }

    public interface IEntityComponent
    {

    }
}