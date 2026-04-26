using Core;
using Entity;
using Player;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SuperDroneAI : BaseDroneAI
    {
        private enum State
        {
            Moving, // 플레이어를 추적하는 상태
            Shooting, // 멈춰서 발사 대기/후딜레이 상태
        }

        [Header("Drone Settings")]
        [SerializeField]
        private float moveDuration = 2f; // 이동 지속 시간

        [SerializeField]
        private float attackRange = 4f; // [추가] 사격 가능한 최대 거리

        public float shootDelay = 1f;

        private State _currentState = State.Moving;

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            _stateTimer = moveDuration;
        }

        protected override void ExecuteBehavior()
        {
            if (playerStat != null)
            {
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;
            }

            switch (_currentState)
            {
                case State.Moving:
                    HandleMovingState();
                    break;
                case State.Shooting:
                    HandleShootingState();
                    break;
            }
        }

        protected override void HandleMovingState()
        {
            base.HandleMovingState();
            // [상태 전환 로직은 기존과 동일]
            if (_stateTimer <= 0)
            {
                float distance = Vector2.Distance(transform.position, playerTarget.position);
                if (distance <= attackRange)
                {
                    _currentState = State.Shooting;
                    _stateTimer = shootDelay;
                    StopMovement();
                    ShootAtPlayer();
                }
                else
                {
                    _stateTimer = 0.5f;
                }
            }
        }

        private void HandleShootingState()
        {
            StopMovement();

            if (_stateTimer <= 0)
            {
                _currentState = State.Moving;
                _stateTimer = moveDuration;
            }
        }

        private void ShootAtPlayer()
        {
            if (stats.data == null || playerTarget == null)
                return;

            var bulletPrefab = stats.GetBulletPrefab("Normal");
            if (bulletPrefab == null)
                return;

            GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
            bullet.transform.position = transform.position;
            Vector2 targetPos = playerTarget.position;
            // 플레이어의 속도를 가져와서 탄환이 날아가는 시간을 고려해 미래 위치 계산
            if (playerStat.TryGetComponent(out Rigidbody2D playerRb))
            {
                float bulletSpeed = stats.ProjectileSpeed.GetValue();
                float dist = Vector2.Distance(transform.position, targetPos);
                float travelTime = dist / bulletSpeed;

                // 미래 위치 = 현재 위치 + (속도 * 이동 시간)
                targetPos += playerRb.linearVelocity * travelTime;
            }

            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (bullet.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    dir,
                    new ProjectileInfo
                    {
                        damage = Mathf.RoundToInt(
                            stats.DamageIncreased.GetValue(stats.AttackDamage.GetValue())
                        ),
                        pierceCount = (int)stats.Pierce.GetValue(),
                        ricochetCount = (int)stats.Ricochet.GetValue(),
                        homingRange = stats.HomingRange.GetValue(),
                        homingStrength = stats.HomingStrength.GetValue(),
                        decelerationRate = stats.DecelerationRate.GetValue(),
                        scale = stats.ProjectileScale.GetValue(),
                        speed = stats.ProjectileSpeed.GetValue(),
                        minSpeed = stats.ProjectileSpeed.GetValue() / 10f,
                    },
                    stats
                );
            }
        }

        // [추가] 에디터에서 사거리를 시각적으로 확인하기 위함
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
