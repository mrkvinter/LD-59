using Code.Game.Scripts.Pawns;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Game.Scripts.EntitySystem
{
    public sealed class EnemyAdapter : EntityAdapter<Entity>
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material damageMaterial;

        private Material defaultMaterial;
        private HealthComponent healthComponent;

        protected override Entity CreateEntity()
        {
            var entity = new Entity(gameObject);

            defaultMaterial = meshRenderer.material;
            healthComponent = new HealthComponent(50);
            entity.AddComponent(healthComponent);

            healthComponent.OnDeath += () => Destroy(gameObject);
            healthComponent.OnDamage += OnDamage;

            return entity;
        }

        private void OnDamage()
        {
            meshRenderer.material = damageMaterial;
            UniTask.Delay(100).ContinueWith(() => meshRenderer.material = defaultMaterial);
        }
    }
}