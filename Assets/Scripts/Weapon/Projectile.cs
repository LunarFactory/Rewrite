using System.Collections.Generic;
using Core;
using Enemy;
using Entity;
using Player;
using UnityEngine;

namespace Weapon
{
    public struct ProjectileInfo
    {
        public int damage;
        public int pierceCount;
        public int ricochetCount;
        public float homingRange;
        public float homingStrength;

        public float decelerationRate;
        public float scale;
        public float speed;
        public float minSpeed;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D rb;
        private int damage;
        public int Damage
        {
            set { damage = value; }
            get { return damage; }
        }
        private bool isInitialized = false;
        private Vector2 moveVelocity;
        private int pierceCount;
        private int ricochetCount;
        private float decelerationRate;
        private float currentSpeed;
        public float CurrentSpeed
        {
            set { currentSpeed = value; }
            get { return currentSpeed; }
        }
        private float minSpeed;

        private bool isHoming = false;
        private float homingRange = 0f; // 유도 인식 범위
        public float HomingRange
        {
            set { homingRange = value; }
            get { return homingRange; }
        }
        private float homingStrength = 0f; // 유도 성능 (높을수록 급격하게 꺾임)
        public float HomingStrength
        {
            set { homingStrength = value; }
            get { return homingStrength; }
        }
        public bool forceNoHoming = false;
        private Transform _target;

        private GameObject _originPrefab;
        private HashSet<EntityId> _hitTargets = new HashSet<EntityId>();

        public bool isSpin = true;

        private EntityStats stats;

        public void SetOriginPrefab(GameObject prefab) => _originPrefab = prefab;

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

        public void Initialize(
            Vector2 direction,
            ProjectileInfo proj,
            EntityStats stats,
            bool noHoming = false
        )
        {
            if (stats is EnemyStats enemy)
            {
                Log.LogTracker.Instance.RegisterEnemyShot();
            }

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            this.moveVelocity = direction.normalized;
            this.stats = stats;

            this.pierceCount = proj.pierceCount;
            this.damage = proj.damage;
            this.ricochetCount = proj.ricochetCount;
            this.homingRange = proj.homingRange;
            this.homingStrength = proj.homingStrength;
            this.decelerationRate = proj.decelerationRate;
            this.currentSpeed = proj.speed;
            this.minSpeed = proj.minSpeed;

            this.transform.localScale *= proj.scale;

            this.isInitialized = true;
            this.forceNoHoming = noHoming;
            UpdateVelocityAndRotation();
        }

        private void FixedUpdate()
        {
            if (homingRange > 0 && homingStrength > 0)
                isHoming = true;
            else
                isHoming = false;
            if (forceNoHoming)
                isHoming = false;
            if (!isInitialized || rb == null)
                return;

            // 1. 유도 대상 찾기 (기존 코드 유지)
            if (isHoming && _target == null)
            {
                _target = FindClosestEnemy();
            }

            // 2. 속도 및 소멸 체크 (기존 코드 유지)
            currentSpeed *= (1 - decelerationRate);
            if (currentSpeed < minSpeed)
            {
                Destroy(gameObject);
                return;
            }

            // 3. [추가] 유도 조향 (Steering) 로직
            if (isHoming && _target != null)
            {
                // 타겟이 죽었는지 한 번 더 체크 (EnemyStats 상속 구조라면)
                if (_target.TryGetComponent<EntityStats>(out var stats))
                {
                    if (stats is EnemyStats enemy && enemy.isDead)
                    {
                        _target = null;
                    }
                }

                if (_target != null)
                {
                    Vector2 directionToTarget = (_target.position - transform.position).normalized;
                    // homingStrength(유도 강도)는 약 5~10 정도를 추천합니다.
                    moveVelocity = Vector2
                        .Lerp(moveVelocity, directionToTarget, homingStrength * Time.fixedDeltaTime)
                        .normalized;

                    // 조향 중에도 탄환의 앞방향이 진행 방향을 바라보게 갱신
                    float angle = Mathf.Atan2(moveVelocity.y, moveVelocity.x) * Mathf.Rad2Deg;
                    if (isSpin)
                        transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }

            // 4. 벽 충돌 검사 (조향된 moveVelocity 기준)
            float moveDistance = currentSpeed * Time.fixedDeltaTime;
            RaycastHit2D hit = Physics2D.CircleCast(
                transform.position,
                0.1f,
                moveVelocity,
                moveDistance,
                LayerMask.GetMask("Obstacle")
            );

            if (hit.collider != null && ricochetCount > 0)
            {
                if (stats is PlayerStats player)
                    player.NotifyWallHit(this);

                // 1. 벽에 부딪힌 순간, 가장 가까운 적을 즉시 새로 찾습니다.
                _target = FindClosestEnemy();

                if (_target != null)
                {
                    // [핵심] 물리적인 반사각(Reflect)을 무시하고, 적을 향한 방향을 곧바로 새 방향으로 설정합니다.
                    Vector2 directionToTarget = ((Vector2)_target.position - hit.point).normalized;
                    moveVelocity = directionToTarget;

                    // 유도 기능을 켠 상태라면, 이제부터는 이 방향으로 날아갑니다.
                    isHoming = true;
                }
                else
                {
                    // 주변에 적이 없다면 일반적인 물리 반사를 수행합니다.
                    Vector2 normal = hit.normal;
                    if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                        normal = new Vector2(Mathf.Sign(normal.x), 0);
                    else
                        normal = new Vector2(0, Mathf.Sign(normal.y));

                    moveVelocity = Vector2.Reflect(moveVelocity, normal).normalized;
                }

                // 2. 위치 보정: 벽 안으로 파고들지 않게 hit.normal 방향으로 살짝 띄워줍니다.
                transform.position = hit.point + (hit.normal * 0.1f);

                // 3. 반사 횟수 차감 및 즉시 회전 업데이트 (중요: 한 프레임에 모든 걸 끝냄)
                ricochetCount--;
                UpdateVelocityAndRotation();
            }
            else if (hit.collider != null && ricochetCount <= 0)
            {
                if (stats is PlayerStats player)
                    player.NotifyWallHit(this);
                Deactivate();
            }
            else
            {
                // 5. 최종 속도 적용
                rb.linearVelocity = moveVelocity * currentSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isInitialized)
                return;

            // [변경] 벽(Obstacle) 체크 로직은 FixedUpdate로 옮겼으므로 여기선 Entity만 체크합니다.
            EntityStats target =
                collision.GetComponent<EntityStats>()
                ?? collision.GetComponentInParent<EntityStats>();

            if (target != null)
            {
                EntityId targetID = target.gameObject.GetEntityId();
                if (_hitTargets.Contains(targetID))
                    return;

                if (target is PlayerStats player && player.isStealth())
                    return;
                if (target is EnemyStats enemy && enemy.isDead)
                    return;

                _hitTargets.Add(targetID);
                _target = null;
                stats.NotifyAttackHit(stats, target, damage);
                stats.NotifyPostAttackHit(stats, target, damage);
                target.TakeDamage(stats, damage);
                HandlePierce();
            }
        }

        private void UpdateVelocityAndRotation()
        {
            rb.linearVelocity = moveVelocity * currentSpeed;
            float angle = Mathf.Atan2(moveVelocity.y, moveVelocity.x) * Mathf.Rad2Deg;
            if (isSpin)
                transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void HandlePierce()
        {
            if (pierceCount != -1)
            {
                // 관통 수치를 깎습니다.
                pierceCount--;

                // 관통 횟수가 다 떨어졌을 때만 파괴합니다.
                // 관통 1이면 두 명을 맞힐 수 있습니다. (첫 번째 맞고 생존, 두 번째 맞고 파괴)
                if (pierceCount < 0)
                {
                    Deactivate();
                }
            }
        }

        private Transform FindClosestEnemy()
        {
            string layer;
            if (stats is EnemyStats)
            {
                layer = "Player";
            }
            else
            {
                layer = "Enemy";
            }
            // 주변 적 레이어만 검색
            Collider2D[] enemies = Physics2D.OverlapCircleAll(
                transform.position,
                homingRange,
                LayerMask.GetMask(layer)
            );

            Transform closest = null;
            float minStatDistance = Mathf.Infinity;

            foreach (var enemy in enemies)
            {
                EntityStats stats =
                    enemy.GetComponent<EntityStats>() ?? enemy.GetComponentInParent<EntityStats>();
                // EnemyStats를 체크해서 죽은 적은 무시 (이전에 만든 기능 활용)
                if (stats == null || (stats is EnemyStats e && e.isDead))
                    continue;
                if (_hitTargets.Contains(enemy.gameObject.GetEntityId()))
                    continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minStatDistance)
                {
                    minStatDistance = dist;
                    closest = enemy.transform;
                }
            }
            return closest;
        }

        public void ResetOnWallHit()
        {
            _hitTargets.Clear();
        }

        public void Deactivate()
        {
            // 이미 비활성화된 상태라면 중복 반납 방지
            if (!gameObject.activeSelf)
                return;

            // ProjectilePooler(또는 Manager)에 자신을 반납
            // _originPrefab은 생성 시점에 저장해둔 프리팹 참조입니다.
            if (_originPrefab != null)
            {
                ProjectileManager.Instance.Release(_originPrefab, gameObject);
            }
            else
            {
                gameObject.SetActive(false); // 폴백
            }
        }
    }
}
