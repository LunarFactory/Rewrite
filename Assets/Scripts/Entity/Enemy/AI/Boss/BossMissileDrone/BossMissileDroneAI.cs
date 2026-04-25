using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossMissileDroneAI : EnemyAI
    {
        private enum State
        {
            Moving,
            RapidMissile,
            Missile,
        }

        [Header("Drone Settings")]
        [SerializeField]
        private float moveDuration = 2f;

        [SerializeField]
        private float attackRange = 30f;

        [SerializeField]
        private float shootDelay = 1.5f; // 일반 탄 1, 2회차 딜레이

        [SerializeField]
        private float thirdShotDelay = 3f; // 10방향 발사 후 딜레이

        [SerializeField]
        private float missileDelay = 0.5f; // 단발 미사일 후딜레이

        [SerializeField]
        private float rapidMissileCooldown = 10f; // 연사 패턴 쿨타임

        [Header("Burst Settings")]
        [SerializeField]
        private int burstCount = 8;

        [SerializeField]
        private float burstInterval = 0.5f;

        private State _currentState = State.Moving;
        private float _stateTimer; // 상태 전환용 타이머
        private float _shotTimer; // 일반 탄 발사 타이머 (상태 무관)
        private float _missileTimer; // 연사 패턴 쿨타임 타이머
        private int _shotsFiredCount; // 일반 탄 발사 횟수 (1, 2, 3회차 체크)
        private int _missilesFired; // 연사 패턴 중 발사된 미사일 수

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            _stateTimer = moveDuration;
            _shotTimer = shootDelay;
            _missileTimer = rapidMissileCooldown;
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

            // 1. 상태와 관계없이 흐르는 타이머들
            _shotTimer -= Time.deltaTime;
            _stateTimer -= Time.deltaTime;
            _missileTimer -= Time.deltaTime;

            // 2. 일반 탄 패턴 (상태와 무관하게 실행)
            HandleNormalShooting();

            // 3. 상태별 행동 (이동 및 미사일)
            switch (_currentState)
            {
                case State.Moving:
                    HandleMovingState();
                    break;
                case State.RapidMissile:
                    HandleRapidMissileState();
                    break;
                case State.Missile:
                    HandleMissileState();
                    break;
            }
        }

        // [패턴 1] 1-1-10 방식의 일반 탄 발사 (상태 무관)
        private void HandleNormalShooting()
        {
            if (_shotTimer <= 0)
            {
                ShootNormalPattern();
            }
        }

        private void ShootNormalPattern()
        {
            _shotsFiredCount++;
            int shotCount = (_shotsFiredCount >= 3) ? 10 : 1;

            var bulletPrefab = stats.GetBulletPrefab("Normal");
            if (bulletPrefab == null)
                return;

            Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

            for (int i = 0; i < shotCount; i++)
            {
                GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
                bullet.transform.position = transform.position;

                // 10방향일 경우 36도씩 회전하며 발사
                float finalAngle = (shotCount == 10) ? baseAngle + (36f * i) : baseAngle;
                bullet.transform.rotation = Quaternion.Euler(0, 0, finalAngle);

                if (bullet.TryGetComponent(out Projectile proj))
                {
                    // 각도에 따른 새로운 방향 벡터 계산
                    float rad = finalAngle * Mathf.Deg2Rad;
                    Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                    proj.Initialize(
                        shootDir,
                        new ProjectileInfo
                        {
                            damage = 5,
                            speed = 10,
                            ricochetCount = 1,
                            scale = 1,
                        },
                        stats
                    );
                }
            }

            // 발사 횟수에 따른 다음 딜레이 설정
            if (_shotsFiredCount >= 3)
            {
                _shotsFiredCount = 0;
                _shotTimer = thirdShotDelay;
            }
            else
            {
                _shotTimer = shootDelay;
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
                    rb.linearVelocity = Vector2.zero;

                    // [패턴 2 & 3 결정] 연사 쿨타임이 끝났는가?
                    if (_missileTimer <= 0)
                    {
                        _currentState = State.RapidMissile;
                        _missilesFired = 0;
                        _stateTimer = 0; // 즉시 첫 발 발사
                    }
                    else
                    {
                        _currentState = State.Missile;
                        _stateTimer = missileDelay;
                        MissileAtPlayer(); // 단발 미사일 즉시 발사
                    }
                }
                else
                {
                    _stateTimer = 0.5f; // 사거리 밖이면 짧게 재탐색
                }
            }
        }

        private void HandleMissileState()
        {
            rb.linearVelocity = Vector2.zero;
            if (_stateTimer <= 0)
            {
                _currentState = State.Moving;
                _stateTimer = moveDuration;
            }
        }

        private void HandleRapidMissileState()
        {
            rb.linearVelocity = Vector2.zero;

            if (_missilesFired < burstCount)
            {
                if (_stateTimer <= 0)
                {
                    MissileAtPlayer();
                    _missilesFired++;

                    // 점점 빨라지는 발사 간격 계산 (0.5 -> 0.45 -> 0.4 ...)
                    _stateTimer = Mathf.Max(0f, burstInterval - (0.05f * _missilesFired));

                    if (_missilesFired >= burstCount)
                    {
                        _missileTimer = rapidMissileCooldown; // 쿨타임 리셋
                        _stateTimer = 2f; // 연사 종료 후 짧은 경직
                    }
                }
            }
            else if (_stateTimer <= 0)
            {
                _currentState = State.Moving;
                _stateTimer = moveDuration;
            }
        }

        private void MissileAtPlayer()
        {
            if (playerTarget == null)
                return;

            var missilePrefab = stats.GetBulletPrefab("Missile");
            if (missilePrefab == null)
                return;

            GameObject missile = ProjectileManager.Instance.Get(missilePrefab);
            missile.transform.position = transform.position;

            Vector2 dir = (playerTarget.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            missile.transform.rotation = Quaternion.Euler(0, 0, angle);

            if (missile.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    dir,
                    new ProjectileInfo
                    {
                        damage = Mathf.RoundToInt(
                            stats.DamageIncreased.GetValue(stats.AttackDamage.GetValue())
                        ),
                        speed = stats.ProjectileSpeed.GetValue(),
                        homingRange = stats.HomingRange.GetValue(),
                        homingStrength = stats.HomingStrength.GetValue(),
                        decelerationRate = stats.DecelerationRate.GetValue(),
                        scale = stats.ProjectileScale.GetValue(),
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
