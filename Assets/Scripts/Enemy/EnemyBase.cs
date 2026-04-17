using UnityEngine;

namespace Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        public float MaxHealth = 30f;
        protected float currentHealth;
        public float MoveSpeed = 2f;
        public float AttackDamage = 10f;
        public float HitStunDuration = 0.15f; // 피격시 경직 시간 (초)
        public bool isInvincible = false; // 더미 등 체력 무한 적용용
        private bool isDead = false;

        protected Transform playerTarget;
        protected float stunTimer;

        public bool IsStunned => stunTimer > 0f;

        protected virtual void Start()
        {
            currentHealth = MaxHealth;
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        protected virtual void Update()
        {
            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
            }
        }

        public virtual void TakeDamage(float damage)
        {
            if (!isInvincible)
            {
                currentHealth -= damage;
            }

            stunTimer = HitStunDuration; // 피격시 경직 적용
            Debug.Log($"{gameObject.name} took {damage} damage, health remaining: {(isInvincible ? "Unlimited" : currentHealth.ToString())}");

            if (currentHealth <= 0 && !isInvincible)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            if (Core.WaveManager.Instance != null)
            {
                Core.WaveManager.Instance.OnEnemyDied();
            }
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
