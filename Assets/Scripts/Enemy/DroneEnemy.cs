using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyEnemy : EnemyBase
    {
        [Header("Combat Settings")]
        [Tooltip("적이 발사할 탄환 프리팹 (EnemyBullet 레이어 권장)")]
        public GameObject bulletPrefab;
        public float bulletSpeed = 10f;

        private Rigidbody2D rb;
        private float stateTimer;
        private bool isMoving = true;

        protected override void Start()
        {
            base.Start();
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            stateTimer = 2f;
        }

        private void FixedUpdate()
        {
            // [추가] 만약 타겟이 없다면 다시 한 번 찾아봅니다.
            if (playerTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTarget = player.transform;
                return; // 이번 프레임은 쉬고 다음 프레임부터 움직입니다.
            }

            // 피격 경직 중에는 정지
            if (IsStunned)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            stateTimer -= Time.fixedDeltaTime;

            if (isMoving)
            {
                Vector2 dir = (playerTarget.position - transform.position).normalized;
                rb.linearVelocity = dir * MoveSpeed;

                if (stateTimer <= 0)
                {
                    isMoving = false;
                    stateTimer = 0.5f;
                    rb.linearVelocity = Vector2.zero;
                    ShootAtPlayer(); // 사격 실행
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;

                if (stateTimer <= 0)
                {
                    isMoving = true;
                    stateTimer = 2f;
                }
            }
        }

        private void ShootAtPlayer()
        {
            // [의존성 제거] TestSetup을 찾지 않고, 자신이 가진 프리팹만 사용합니다.
            if (bulletPrefab == null)
            {
                Debug.LogWarning($"[DummyEnemy] {gameObject.name}의 bulletPrefab이 할당되지 않았습니다!");
                return;
            }

            Vector2 dir = (playerTarget.position - transform.position).normalized;

            // 탄환 생성
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.SetActive(true);

            // 탄환 초기화 (isPlayer = false)
            if (bullet.TryGetComponent(out Weapons.Projectile proj))
            {
                proj.Initialize(dir, bulletSpeed, bulletSpeed / 5, AttackDamage, 0, false);
            }
        }
    }
}