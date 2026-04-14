using UnityEngine;

namespace Code.Game.Scripts.EntitySystem
{
    public interface IEntityComponentAdapter
    {
        void Initialize(Entity entity);
    }

    public abstract class EntityComponentAdapter<TComponent> : MonoBehaviour, IEntityComponentAdapter
        where TComponent : IEntityComponent
    {
        protected TComponent Component;
        protected Entity Entity;

        public void Initialize(Entity entity)
        {
            Entity = entity;
            Component = CreateComponent();
            Entity.AddComponent(Component);
        }

        protected abstract TComponent CreateComponent();
    }

    public abstract class EntityComponentAdapter<TComponent, TEntity> : MonoBehaviour, IEntityComponentAdapter
        where TComponent : IEntityComponent
        where TEntity : Entity
    {
        protected TComponent Component;
        protected TEntity Entity;

        public void Initialize(Entity entity)
        {
            if (entity is not TEntity castedEntity)
            {
                Debug.LogError($"{GetType().Name} needs a compatible entity type: {entity.GetType().Name} vs {typeof(TEntity).Name}");
                return;
            }
            Entity = castedEntity;
            Component = CreateComponent();
            Entity.AddComponent(Component);
        }

        protected abstract TComponent CreateComponent();
    }
}