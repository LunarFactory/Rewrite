using UnityEngine;
using Entity;
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
        private Vector2 moveVelocity;
        private int currentPierce;
        private float currentSpeed;
        private float minSpeed;
        public float decelerationRate = 1.00f; // 1.0이면 유지, 작을수록 빨리 느려짐
        [HideInInspector] public EntityStats ownerStats; // [핵심] 신호를 보낼 대상

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

        public void Initialize(Vector2 direction, float speed, float minSpeed, int damage, int pierceCount, EntityStats stats)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            if (speed <= 0f) speed = minSpeed + 0.1f;
            if (damage <= 0) damage = 1;

            this.moveVelocity = direction.normalized;
            this.currentPierce = pierceCount;
            this.damageVal = Mathf.RoundToInt(stats.DamageIncreased.GetValue(damage));
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

            // 1. 공통 EntityStatus 추출 (자신 혹은 부모)
            EntityStats target = collision.GetComponent<EntityStats>() ?? collision.GetComponentInParent<EntityStats>();

            if (target != null)
            {
                if (target is PlayerStats player)
                {
                    if (player.isStealth()) return;
                }
                if (target is EnemyStats enemy)
                {
                    if (enemy.isDead) return;
                }
                Debug.Log($"피해량은 다음과 같습니다 : {Mathf.RoundToInt(target.DamageTaken.GetValue(damageVal))}");
                ownerStats.NotifyAttackHit(ownerStats, target, damageVal);
                target.TakeDamage(ownerStats, damageVal);
                HandlePierce();
                // [적 투사체 -> 플레이어 타격]

            }

            // 2. 장애물 충돌 (벽 등)
            if (collision.CompareTag("Obstacle"))
            {
                if (ownerStats != null)
                {
                    if (ownerStats is PlayerStats player)
                    {
                        player.NotifyWallHit();
                    }
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