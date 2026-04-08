using UnityEngine;

namespace Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        public float MaxHealth = 30f;
        protected float currentHealth;
        public float MoveSpeed = 2f;
        public float AttackDamage = 10f;
        
        protected Transform playerTarget;

        protected virtual void Start()
        {
            currentHealth = MaxHealth;
            // Find player
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTarget = player.transform;
        }

        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage, health remaining: {currentHealth}");
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            Destroy(gameObject);
        }
        
        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            // Continuous damage if touching player
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.gameObject.TryGetComponent(out Player.PlayerStats stats))
                {
                    stats.TakeDamage(AttackDamage * Time.fixedDeltaTime);
                }
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            // Initial impact damage
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.gameObject.TryGetComponent(out Player.PlayerStats stats))
                {
                    stats.TakeDamage(AttackDamage);
                }
            }
        }
    }
}
