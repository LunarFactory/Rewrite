using System.Collections;
using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossGatlingDroneAI : BaseDroneAI
    {
        private enum State
        {
            Moving, // 플레이어 추적 및 연사
            SpreadBurst, // 5방향(혹은 8방향) 산탄 패턴
            Circular, // 원형 발사 패턴
        }

        [Header("Gatling Settings")]
        [SerializeField]
        private float rapidFireRate = 0.1f;

        [SerializeField]
        private float moveDuration = 2f;

        [Header("Pattern Cooldowns")]
        [SerializeField]
        private float spreadCooldown = 5f;

        [SerializeField]
        private float circularCooldown = 10f;

        private State _currentState = State.Moving;
        private bool _isPhase2 = false;

        // 타이머 및 카운터
        private float _gatlingTimer;
        private float _spreadTimer;
        private float _circularTimer;

        private int _spreadBurstRemaining = 0; // 산탄 남은 횟수 (4회)

        protected override void Awake()
        {
            base.Awake();
            _currentState = State.Moving;
            _stateTimer = moveDuration;
            _spreadTimer = spreadCooldown;
            _circularTimer = circularCooldown;
        }

        protected override void ExecuteBehavior()
        {
            // 타겟 체크 및 상태 이상 체크
            if (playerStat != null)
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;

            // 페이즈 체크 (체력 50% 이하)
            CheckPhase();

            // 타이머 업데이트 (2페이즈 시 패턴 속도 증가)
            float dt = Time.deltaTime;
            float speedMultiplier = _isPhase2 ? 1.5f : 1f;

            _spreadTimer -= dt * speedMultiplier;
            _circularTimer -= dt * speedMultiplier;

            // 상태 우선순위 결정 (원형 > 산탄 > 일반)
            HandleStateTransitions();

            // 상태별 행동
            switch (_currentState)
            {
                case State.Moving:
                    HandleMovingAndGatling();
                    break;
                case State.SpreadBurst:
                    HandleSpreadBurstState();
                    break;
                case State.Circular:
                    HandleCircularState();
                    break;
            }
        }

        private void CheckPhase()
        {
            if (!_isPhase2 && stats.currentHealth <= stats.maxHealth * 0.5f)
            {
                _isPhase2 = true;
                stats.MoveSpeed.AddModifier(
                    new StatModifier("GatlingDronePhase2", 0.2f, ModifierType.Percent, this)
                );
            }
        }

        private void HandleStateTransitions()
        {
            // 10초마다 무조건 원형 발사 (최우선)
            if (_circularTimer <= 0)
            {
                _currentState = State.Circular;
                _circularTimer = circularCooldown;
                _stateTimer = 1.5f; // 원형 발사 시 잠시 멈춤
                ShootCircular();
                return;
            }

            // 5초마다 산탄 발사 시작
            if (_currentState == State.Moving && _spreadTimer <= 0)
            {
                _currentState = State.SpreadBurst;
                _spreadTimer = spreadCooldown;
                _spreadBurstRemaining = 4; // 4회 발사 예약
                _stateTimer = 0; // 즉시 첫 발
            }
        }

        // [패턴 1] 이동하며 플레이어 방향 연사
        private void HandleMovingAndGatling()
        {
            // 이동 로직
            base.HandleMovingState();

            // 연사 로직
            Vector2 dir = (playerTarget.position - transform.position).normalized;

            _gatlingTimer -= Time.deltaTime;
            if (_gatlingTimer <= 0)
            {
                _gatlingTimer = rapidFireRate / (_isPhase2 ? 1.5f : 1f);
                ShootProjectile(dir, "Normal");
            }
        }

        // [패턴 2] 5방향/8방향 산탄 4회 발사
        private void HandleSpreadBurstState()
        {
            rb.linearVelocity = Vector2.zero; // 발사 중 정지

            if (_stateTimer <= 0 && _spreadBurstRemaining > 0)
            {
                ShootSpread();
                _spreadBurstRemaining--;
                _stateTimer = 0.3f / (_isPhase2 ? 1.5f : 1f); // 버스트 간격

                if (_spreadBurstRemaining <= 0)
                {
                    _currentState = State.Moving;
                    _stateTimer = moveDuration;
                }
            }
        }

        // [패턴 3] 원형 발사 후 대기
        private void HandleCircularState()
        {
            rb.linearVelocity = Vector2.zero;

            if (_stateTimer <= 0)
            {
                _currentState = State.Moving;
                _stateTimer = moveDuration;
            }
        }

        private void ShootSpread()
        {
            int count = _isPhase2 ? 6 : 4;
            float spreadAngle = 45f;

            Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - (spreadAngle / 2f);
            float angleStep = spreadAngle / (count - 1);

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (angleStep * i);
                ShootProjectileFromAngle(angle, "Normal");
            }
        }

        private void ShootCircular()
        {
            int count = _isPhase2 ? 20 : 10;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                ShootProjectileFromAngle(i * angleStep, "Normal");
            }
        }

        // 헬퍼 함수: 각도 기반 발사
        private void ShootProjectileFromAngle(float angle, string type)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            ShootProjectile(dir, type, angle);
        }

        private void ShootProjectile(Vector2 dir, string type, float? forcedAngle = null)
        {
            var prefab = stats.GetBulletPrefab(type);
            if (prefab == null)
                return;

            GameObject bullet = ProjectileManager.Instance.Get(prefab);
            bullet.transform.position = transform.position;

            float angle = forcedAngle ?? Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
                        speed = stats.ProjectileSpeed.GetValue() * (_isPhase2 ? 1.2f : 1f),
                        ricochetCount = 0,
                        scale = stats.ProjectileScale.GetValue(),
                    },
                    stats
                );
            }
        }
    }
}
