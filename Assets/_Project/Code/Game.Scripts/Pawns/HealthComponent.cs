using System;
using Code.Game.Scripts.EntitySystem;

namespace Code.Game.Scripts.Pawns
{
    public sealed class HealthComponent : IEntityComponent
    {
        private float currentHealth;
        private float maxHealth;

        public event Action OnDamage; 
        public event Action OnDeath;

        public float CurrentHealth => currentHealth;

        public float MaxHealth => maxHealth;

        public HealthComponent(float maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            OnDamage?.Invoke();
            if (CurrentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke();
            }
        }
    }
}