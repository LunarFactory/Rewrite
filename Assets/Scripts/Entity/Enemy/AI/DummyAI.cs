using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyAI : EnemyAI
    {
        protected override void Update() { }

        protected override void ExecuteBehavior()
        {
            if (stats.currentHealth < stats.maxHealth)
            {
                stats.currentHealth = stats.maxHealth;
            }
        }
    }
}
