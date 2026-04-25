using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SuperMissileDroneAI : EnemyAI
    {
        private enum State
        {
            Moving,
            Shooting,
        }

        [Header("Drone Settings")]
        [SerializeField]
        private float moveDuration = 2f;

        [SerializeField]
        private float attackRange = 12f;

        [SerializeField]
        private float shootDelay = 1.5f; // 점사 후 다음 이동까지의 후딜레이

        [Header("Burst Settings")]
        [SerializeField]
        private int burstCount = 3; // 한 번에 쏠 횟수

        [SerializeField]
        private float burstInterval = 0.2f; // 총알 사이의 간격

        private State _currentState = State.Moving;
        private float _stateTimer;
        private int _shotsFired; // 현재 발사된 탄환 수

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            _stateTimer = moveDuration;
        }

        protected override void ExecuteBehavior()
        {
            if (playerStat != null)
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;

            if (playerTarget == null || stats.isStaggered)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            _stateTimer -= Time.deltaTime;

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

        private void HandleMovingState()
        {
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * stats.MoveSpeed.GetValue();

            if (_stateTimer <= 0)
            {
                float distance = Vector2.Distance(transform.position, playerTarget.position);

                if (distance <= attackRange)
                {
                    // 점사 시작 설정
                    _currentState = State.Shooting;
                    _shotsFired = 0;
                    _stateTimer = 0; // 즉시 첫 발 발사하도록 설정
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    _stateTimer = 0.5f;
                }
            }
        }

        private void HandleShootingState()
        {
            rb.linearVelocity = Vector2.zero;

            // 아직 점사가 끝나지 않았을 때
            if (_shotsFired < burstCount)
            {
                if (_stateTimer <= 0)
                {
                    ShootAtPlayer();
                    _shotsFired++;

                    // 다음 발사까지의 짧은 간격 설정
                    _stateTimer = burstInterval;

                    // 마지막 발사였다면 후딜레이 적용
                    if (_shotsFired >= burstCount)
                    {
                        _stateTimer = shootDelay;
                    }
                }
            }
            // 모든 발사와 후딜레이가 끝났을 때
            else if (_stateTimer <= 0)
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
