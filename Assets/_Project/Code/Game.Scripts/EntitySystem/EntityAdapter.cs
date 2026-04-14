using System;
using Code.Game.Core;
using Game.Core.Contexts;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Code.Game.Scripts.EntitySystem
{
    public interface IEntityAdapter
    {
        Entity BaseEntity { get; }
    }

    public abstract class EntityAdapter<TEntity> : ObjectRegistry.MonoRegistered<EntityAdapter<TEntity>>, IEntityAdapter
        where TEntity : Entity
    {
        private EntityService entityService;

        public Entity BaseEntity => Entity;
        public TEntity Entity { get; private set; }

        protected override void OnInitialize()
        {
            Entity = CreateEntity();
            foreach (var entityComponentAdapter in gameObject.GetComponentsInChildren<IEntityComponentAdapter>())
            {
                try
                {
                    entityComponentAdapter.Initialize(Entity);
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }
            
            entityService = GameServiceLocator.Resolve<EntityService>();
            entityService.AddEntity(Entity);
            
            OnEntityInitialized();
        }
        
        protected abstract TEntity CreateEntity();

        protected virtual void OnEntityInitialized()
        {
        }

        private void Update()
        {
            if (Entity is ITickable tickable) tickable.Tick();
        }


        public void Destroy()
        {
            entityService.RemoveEntity(Entity);
            Object.Destroy(gameObject);
        }
    }
}