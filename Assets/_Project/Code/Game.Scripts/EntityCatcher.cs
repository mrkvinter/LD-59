using System.Collections.Generic;
using System.Linq;
using Code.Game.Scripts.EntitySystem;
using UnityEngine;

namespace Code.Game.Scripts
{
    public class EntityCatcher
    {
        private readonly Collider[] colliders = new Collider[100];

        public IReadOnlyList<Entity> GetEntitiesAt(Vector3 position, float radius)
        {
            var result = new List<Entity>();
            var size = Physics.OverlapSphereNonAlloc(position, radius, colliders);
            for (int i = 0; i < size; i++)
            {
                if (colliders[i].TryGetComponent<IEntityAdapter>(out var entityAdapter))
                {
                    result.Add(entityAdapter.BaseEntity);
                }
            }

            return result.OrderBy(e => (position - e.Transform.position).sqrMagnitude).ToList();
        }
    }
}