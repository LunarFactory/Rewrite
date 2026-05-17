using System.Collections;
using Core;
using UnityEngine;
using Weapon;

namespace Enemy
{
    public class BossOmegaAI : BaseDroneAI
    {
        private enum State
        {
            Idle,
            Pattern_Summon,
            Pattern_Homing,
            Pattern_Circle,
            Pattern_Shotgun,
        }

        [Header("Omega Settings")]
        [SerializeField]
        private EnemyData suicideDroneData; // 자폭 드론 데이터

        [SerializeField]
        private float patternInterval = 0.5f; // 패턴 사이 대기 시간
        private State _currentState = State.Idle;

        // [추가] 플레이어가 숨었을 때 마지막으로 포착된 방향을 저장하는 징검다리 변수
        private Vector2 _lastKnownDir = Vector2.down;

        protected override void Awake()
        {
            base.Awake();
            // 고정형 보스이므로 이동 엔진 정지
            if (aiPath != null)
                aiPath.canMove = false;
            _stateTimer = patternInterval;
            suicideDroneData = Resources.Load<EnemyData>("Enemies/Drones/BombDrone");
        }

        protected override void ExecuteBehavior()
        {
            UpdateTargetByStealth();

            // [수정] 상단에 있던 'if (playerTarget == null) return;' 유무를 하단 타이머 조건문으로 이관합니다.
            // 그래야 타겟이 없어도 Idle 상태에서 타이머가 정상적으로 깎여 다음 행동을 취합니다.
            if (_currentState == State.Idle)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0)
                {
                    SelectRandomPattern();
                }
            }
        }

        private void SelectRandomPattern()
        {
            // 4가지 패턴 중 하나를 무작위 선택
            int random = Random.Range(1, 5);
            _currentState = (State)random;
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }

            switch (_currentState)
            {
                case State.Pattern_Summon:
                    StartCoroutine(SummonSuicideDrones());
                    break;
                case State.Pattern_Homing:
                    StartCoroutine(LaunchHomingMissiles());
                    break;
                case State.Pattern_Circle:
                    StartCoroutine(CircularBurst());
                    break;
                case State.Pattern_Shotgun:
                    StartCoroutine(ShotgunBurst());
                    break;
            }
        }

        // 패턴 1: 자폭 드론 5마리 소환 (타겟 위치 정보 불필요하므로 무관)
        private IEnumerator SummonSuicideDrones()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
                GameManager.Instance.ExecuteSpawn(suicideDroneData, false, spawnPos);
                yield return new WaitForSeconds(0.2f);
            }
            EndPattern();
        }

        // 패턴 2: 유도 미사일 발사
        private IEnumerator LaunchHomingMissiles()
        {
            var missilePrefab = stats.GetBulletPrefab("Missile");
            for (int i = 0; i < 3; i++) // 3발 발사
            {
                FireProjectile(missilePrefab, GetDirToPlayer(), true);
                yield return new WaitForSeconds(0.5f);
            }
            EndPattern();
        }

        // 패턴 3: 원형 탄환 10개 (3회 반복 / 타겟 불필요)
        private IEnumerator CircularBurst()
        {
            var bulletPrefab = stats.GetBulletPrefab("Normal");
            for (int burst = 0; burst < 3; burst++)
            {
                for (int i = 0; i < 10; i++)
                {
                    float angle = i * 36f;
                    Vector2 dir = new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad),
                        Mathf.Sin(angle * Mathf.Deg2Rad)
                    );
                    FireProjectile(bulletPrefab, dir, false);
                }
                yield return new WaitForSeconds(0.8f);
            }
            EndPattern();
        }

        // 패턴 4: 산탄 5발 플레이어 방향 (3회 반복)
        private IEnumerator ShotgunBurst()
        {
            var bulletPrefab = stats.GetBulletPrefab("Normal");
            for (int burst = 0; burst < 3; burst++)
            {
                Vector2 baseDir = GetDirToPlayer();
                float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

                for (int i = -2; i <= 2; i++) // 5발 산탄
                {
                    float angle = baseAngle + (i * 15f); // 15도 간격
                    Vector2 dir = new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad),
                        Mathf.Sin(angle * Mathf.Deg2Rad)
                    );
                    FireProjectile(bulletPrefab, dir, false);
                }
                yield return new WaitForSeconds(0.6f);
            }
            EndPattern();
        }

        private void FireProjectile(GameObject prefab, Vector2 dir, bool isHoming)
        {
            if (prefab == null)
                return;
            GameObject bullet = ProjectileManager.Instance.Get(prefab);
            bullet.transform.position = transform.position;

            if (bullet.TryGetComponent(out Projectile proj))
            {
                proj.Initialize(
                    dir,
                    new ProjectileInfo
                    {
                        damage = (int)stats.AttackDamage.GetValue(),
                        speed = isHoming ? stats.ProjectileSpeed.GetValue() : 10f,
                        ricochetCount = isHoming ? (int)stats.Ricochet.GetValue() : 1,
                        decelerationRate = isHoming ? stats.DecelerationRate.GetValue() : 0,
                        homingRange = isHoming ? stats.HomingRange.GetValue() : 0,
                        homingStrength = isHoming ? stats.HomingStrength.GetValue() : 0,
                        scale = stats.ProjectileScale.GetValue(),
                    },
                    stats
                );
            }
        }

        private void EndPattern()
        {
            _currentState = State.Idle;
            _stateTimer = patternInterval;
        }

        // [수정] 플레이어가 은신해도 크래시가 나지 않고 마지막 조준 방향을 리턴하는 예외 안전 기법
        private Vector2 GetDirToPlayer()
        {
            if (playerTarget != null)
            {
                _lastKnownDir = (playerTarget.position - transform.position).normalized;
            }
            return _lastKnownDir;
        }
    }
}
