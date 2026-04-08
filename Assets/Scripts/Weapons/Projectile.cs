using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private float speed;
        private float damage;
        private int pierceRemaining;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 direction, float speed, float damage, int pierceCount)
        {
            this.speed = speed;
            this.damage = damage;
            this.pierceRemaining = pierceCount;
            
            rb.gravityScale = 0f;
            rb.linearVelocity = direction.normalized * speed;
            
            Destroy(gameObject, 5f); // Auto destroy after 5 seconds
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                if (collision.TryGetComponent(out Enemy.EnemyBase enemy))
                {
                    enemy.TakeDamage(damage);
                    Log.PlayerLogManager.Instance?.RecordShotHit();
                }
            }

            if (collision.CompareTag("Obstacle") || collision.CompareTag("Enemy"))
            {
                if (pierceRemaining > 0)
                {
                    pierceRemaining--;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
