using Core;
using Entity;
using Player;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DroneAI : EnemyAI
    {
        private enum State
        {
            Moving, // 플레이어를 추적하는 상태
            Shooting, // 멈춰서 발사 대기/후딜레이 상태
        }

        [Header("Drone Settings")]
        [SerializeField]
        private float moveDuration = 2f; // 이동 지속 시간

        public float shootDelay = 0.5f;

        private State _currentState = State.Moving;
        private float _stateTimer;

        private PlayerStats playerStat;
        private Transform playerTarget;

        protected override void Awake()
        {
            base.Awake(); // 부모(EnemyAI)의 참조 할당 로직 실행 (stats, rb, playerTarget 등)

            // 초기 상태 및 타이머 설정
            _currentState = State.Moving;
            _stateTimer = moveDuration;
        }

        protected void Start()
        {
            if (PlayerStats.LocalPlayer != null)
            {
                playerStat = PlayerStats.LocalPlayer;
                playerTarget = playerStat.transform;
            }
        }

        protected override void ExecuteBehavior()
        {
            // 플레이어 은신 체크
            if (playerStat != null)
            {
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;
            }
            // 1. 타겟 확인 (EnemyStats에서 관리하는 playerTarget 사용)
            if (playerTarget == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // 2. 경직 처리 (EnemyStats의 isStaggered 체크)
            if (stats.isStaggered)
            {
                rb.linearVelocity = Vector2.zero;
                return;
                // 경직 중에는 타이머도 줄이지 않고 로직을 중단합니다.
            }

            // 3. 상태 머신 타이머 업데이트
            _stateTimer -= Time.deltaTime;

            // 4. 상태별 로직 실행
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
            // 플레이어 방향으로 이동
            Vector2 dir = (playerTarget.position - transform.position).normalized;

            // stats(EnemyStats)에 저장된 이동 속도 적용
            rb.linearVelocity = dir * stats.MoveSpeed.GetValue();

            if (_stateTimer <= 0)
            {
                // 공격 상태로 전환
                _currentState = State.Shooting;

                // EnemyData에 정의된 사격 지연시간(후딜레이) 적용
                _stateTimer = shootDelay;

                rb.linearVelocity = Vector2.zero; // 멈춰서 사격
                ShootAtPlayer();
            }
        }

        private void HandleShootingState()
        {
            // 발사 후 멈춰있는 상태 (반동이나 후딜레이 표현)
            rb.linearVelocity = Vector2.zero;

            if (_stateTimer <= 0)
            {
                // 다시 이동 상태로 전환
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

            // 총알 생성
            GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
            bullet.transform.position = transform.position;

            Vector2 dir = (playerTarget.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 투사체 초기화 (stats의 모든 CharacterStat 적용)
            if (bullet.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    dir,
                    new ProjectileInfo
                    {
                        // stats.DamageIncreased와 stats.AttackDamage를 조합한 최종 데미지
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
                    stats // 발사체 주인으로 현재 stats 전달
                );
            }
        }
    }
}
