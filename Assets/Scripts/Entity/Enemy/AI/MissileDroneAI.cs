using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MissileDroneAI : BaseDroneAI
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
        private float attackRange = 12f; // [추가] 사격 가능한 최대 거리

        public float shootDelay = 2f;

        private State _currentState = State.Moving;

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            _stateTimer = moveDuration;
        }

        protected override void ExecuteBehavior()
        {
            if (playerTarget == null)
                return;

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
            // [수정] 이동 타이머가 끝났고, 플레이어가 사거리 이내일 때만 발사
            if (_stateTimer <= 0)
            {
                float distance = Vector2.Distance(transform.position, playerTarget.position);

                if (distance <= attackRange)
                {
                    // 사거리 안이면 공격 상태로 전환
                    _currentState = State.Shooting;
                    _stateTimer = shootDelay;
                    StopMovement();
                    ShootAtPlayer();
                }
                else
                {
                    // 사거리 밖이면 이동 타이머만 초기화하고 계속 추적
                    _stateTimer = 0.5f; // 너무 자주 체크하지 않도록 약간의 유예를 줌
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

            Vector2 dir = (playerTarget.position - transform.position).normalized;
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
