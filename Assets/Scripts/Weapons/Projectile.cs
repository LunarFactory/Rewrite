using UnityEngine;
using Player; // PlayerStats 참조를 위해 필요
using Enemy;  // EnemyBase 참조를 위해 필요

namespace Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D rb;
        private int damageVal;
        private bool isInitialized = false;
        private bool isPlayerProjectile = true;
        private Vector2 moveVelocity;
        private int currentPierce;
        private float currentSpeed;
        private float minSpeed;
        public float decelerationRate = 1.00f; // 1.0이면 유지, 작을수록 빨리 느려짐
        [HideInInspector] public PlayerStats ownerStats; // [핵심] 신호를 보낼 대상

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }
        }

        public void Initialize(Vector2 direction, float speed, float minSpeed, int damage, int pierceCount, bool isPlayer, PlayerStats stats)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            if (speed <= 0f) speed = 20f;
            if (damage <= 0) damage = 10;

            this.moveVelocity = direction.normalized;
            this.currentPierce = pierceCount;
            this.damageVal = damage;
            this.isPlayerProjectile = isPlayer;
            this.currentSpeed = speed;
            this.minSpeed = minSpeed;
            this.ownerStats = stats;
            this.isInitialized = true;

            rb.linearVelocity = moveVelocity * currentSpeed;
#if !UNITY_2023_1_OR_NEWER
            rb.velocity = moveVelocity;
#endif

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, 3f);
        }

        private void FixedUpdate()
        {
            if (!isInitialized || rb == null) return;

            // [수정] 매 프레임 속도를 조금씩 줄임
            currentSpeed *= decelerationRate;

            // 최소 속도 아래로 내려가지 않게 방어 (너무 느려지면 이상하니까요)
            if (currentSpeed < minSpeed)
            {
                Destroy(gameObject); // 혹은 아주 느려지면 소멸
                return;
            }

            rb.linearVelocity = moveVelocity * currentSpeed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isInitialized) return;

            // [개선] 더 확실한 컴포넌트 감지 로직
            if (isPlayerProjectile)
            {
                // 적군인지 확인 (자신 혹은 부모에게 EnemyBase가 있는지)
                var enemy = collision.GetComponent<Enemy.EnemyBase>() ?? collision.GetComponentInParent<Enemy.EnemyBase>();

                if (enemy != null && !enemy.isDead)
                {
                    enemy.TakeDamage(damageVal);
                    if (ownerStats != null)
                    {
                        ownerStats.NotifyAttackHit(enemy, damageVal);
                    }

                    HandlePierce();
                }
            }
            else
            {
                // 플레이어인지 확인
                var playerStats = collision.GetComponent<Player.PlayerStats>() ?? collision.GetComponentInParent<Player.PlayerStats>();
                if (playerStats != null)
                {
                    if (playerStats.isStealth()) return;
                    playerStats.TakeDamage(damageVal);
                    HandlePierce();
                }
            }

            // 장애물 충돌 (벽 등)
            if (collision.CompareTag("Obstacle"))
            {
                if (isPlayerProjectile && ownerStats != null)
                {
                    ownerStats.NotifyWallHit();
                }
                Destroy(gameObject);
            }
        }

        private void HandlePierce()
        {
            if (currentPierce <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                currentPierce--;
            }
        }
    }
}