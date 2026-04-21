using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Dummy : EnemyStats
    {
        protected override void Update()
        {
            if (currentHealth < maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }
}