using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D rb;
        private float damageVal;
        private bool isInitialized = false;
        private bool isPlayerProjectile = true;
        private Vector2 moveVelocity;
        private int currentPierce;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Ensure the collider is set as a trigger so it doesn't physically bounce or get blocked by the Player/other objects
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }
        }

        public void Initialize(Vector2 direction, float speed, float damage, int pierceCount, bool isPlayer = true)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            
            // Fallback just in case ProjectileSpeed in WeaponData is 0
            if (speed <= 0f) speed = 20f;
            if (damage <= 0f) damage = 10f;
            
            moveVelocity = direction.normalized * speed;
            this.currentPierce = pierceCount;
            this.damageVal = damage;
            this.isPlayerProjectile = isPlayer;
            this.isInitialized = true;
            
            // Apply initial velocity immediately
            rb.linearVelocity = moveVelocity;
            #if !UNITY_2023_1_OR_NEWER
            rb.velocity = moveVelocity;
            #endif
            
            // Auto-align sprite to direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, 3f); // Bullet life span
        }

        private void FixedUpdate()
        {
            if (!isInitialized || rb == null) return;
            
            // Enforce velocity continuously to override any arbitrary physics stopping
            rb.linearVelocity = moveVelocity;
            #if !UNITY_2023_1_OR_NEWER
            rb.velocity = moveVelocity;
            #endif
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isInitialized) return;

            // Player projectiles only affect Enemies
            if (isPlayerProjectile)
            {
                var enemy = collision.GetComponentInParent<Enemy.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageVal);
                    Log.PlayerLogManager.Instance?.RecordShotHit();

                    if (currentPierce <= 0) Destroy(gameObject);
                    else currentPierce--;
                }
            }
            // Enemy projectiles only affect Players
            else
            {
                var playerStats = collision.GetComponentInParent<Player.PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damageVal);

                    if (currentPierce <= 0) Destroy(gameObject);
                    else currentPierce--;
                }
            }

            // Both destroy on obstacles unconditionally
            if (collision.CompareTag("Obstacle"))
            {
                Destroy(gameObject);
            }
        }
    }
}
