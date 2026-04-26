using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    public class BossGarageDroneAI : BaseDroneAI
    {
        private enum State
        {
            Moving,
            Summon,
        }

        [Header("Summon Settings")]
        [SerializeField]
        private EnemyData[] summonPool; // 인스펙터에서 드론 데이터들을 넣어주세요

        [SerializeField]
        private int spawnCount = 10; // 한 번에 소환할 마릿수

        [SerializeField]
        private float summonInterval = 8f;
        private float _summonTimer;

        [Header("Attack Settings")]
        [SerializeField]
        private float _shotDelay = 1f;
        private float _shotTimer;

        private State _currentState = State.Moving;

        protected override void Awake()
        {
            base.Awake();
            _summonTimer = summonInterval;
            _shotTimer = _shotDelay;
            summonPool = Resources.LoadAll<EnemyData>("Enemies/Drones");
        }

        protected override void ExecuteBehavior()
        {
            UpdateTargetByStealth();

            _shotTimer -= Time.deltaTime;
            _summonTimer -= Time.deltaTime;

            if (_shotTimer <= 0 && playerTarget != null)
                HandleShootState();

            // 소환 타이머 체크 및 상태 전환
            if (_summonTimer <= 0 && _currentState == State.Moving)
            {
                _currentState = State.Summon;
            }

            switch (_currentState)
            {
                case State.Moving:
                    HandleMovingState();
                    break;
                case State.Summon:
                    HandleSummonState();
                    break;
            }
        }

        private void HandleSummonState()
        {
            StopMovement(); // 소환 중 정지

            if (summonPool != null && summonPool.Length > 0)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    // 1. 무작위 데이터 선택
                    EnemyData selectedData = summonPool[Random.Range(0, summonPool.Length)];

                    // 2. 보스 주변 랜덤 위치 계산
                    Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 2f;

                    // 3. [핵심] GameManager의 메서드 호출
                    // 소환되는 드론들은 보스가 아니므로 isBoss는 false로 보냅니다.
                    GameManager.Instance.ExecuteSpawn(selectedData, false, spawnPos);
                }

                Debug.Log($"<color=yellow>[Boss]</color> {spawnCount}기의 드론을 사출했습니다!");
            }

            _summonTimer = summonInterval;
            _currentState = State.Moving;
            ResumeMovement();
        }

        private void HandleShootState()
        {
            var bulletPrefab = stats.GetBulletPrefab("Normal");
            if (bulletPrefab == null || playerTarget == null)
                return;

            Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

            for (int i = 0; i < 10; i++)
            {
                GameObject bullet = ProjectileManager.Instance.Get(bulletPrefab);
                bullet.transform.position = transform.position;

                float finalAngle = baseAngle + (360f / 10 * i);
                bullet.transform.rotation = Quaternion.Euler(0, 0, finalAngle);

                if (bullet.TryGetComponent(out Projectile proj))
                {
                    float rad = finalAngle * Mathf.Deg2Rad;
                    Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                    proj.Initialize(
                        shootDir,
                        new ProjectileInfo
                        {
                            damage = 10,
                            speed = 8,
                            ricochetCount = 4,
                            minSpeed = 1,
                            scale = stats.ProjectileScale.GetValue(),
                        },
                        stats
                    );
                }
            }
            _shotTimer = _shotDelay;
        }
    }
}
