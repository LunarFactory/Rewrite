using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DroneEnemy : EnemyBase
    {
        private enum State { Moving, Shooting, Stunned, Staggered }
        private State currentState = State.Moving;

        private float stateTimer;

        protected override void Start()
        {
            base.Start();
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            // 초기 상태 설정
            if (data != null) stateTimer = data.moveDuration;
        }

        protected override void OnFixedUpdate()
        {
            // 1. 타겟 확인 (부모의 playerTarget 활용)
            if (playerTarget == null) return;

            // 2. 경직 처리 (상태를 명시적으로 분리하면 관리가 쉽습니다)

            // 3. 상태별 로직
            stateTimer -= Time.fixedDeltaTime;

            switch (currentState)
            {
                case State.Moving:
                    HandleMovingState();
                    break;

                case State.Shooting:
                    HandleShootingState();
                    break;
            }
            if (IsStaggered)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleMovingState()
        {
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * data.moveSpeed; // data에서 가져옴

            if (stateTimer <= 0)
            {
                currentState = State.Shooting;
                stateTimer = data.shootDelay;
                rb.linearVelocity = Vector2.zero;
                ShootAtPlayer();
            }
        }

        private void HandleShootingState()
        {
            // 발사 후 잠시 정지해 있는 상태
            rb.linearVelocity = Vector2.zero;

            if (stateTimer <= 0)
            {
                currentState = State.Moving;
                stateTimer = data.moveDuration;
            }
        }

        private void ShootAtPlayer()
        {
            // data는 EnemyData(ScriptableObject)를 참조하고 있다고 가정합니다.
            if (data == null || data.bulletPrefab == null) return;

            Vector2 dir = (playerTarget.position - transform.position).normalized;

            GameObject bullet = Instantiate(data.bulletPrefab, transform.position, Quaternion.identity);

            if (bullet.TryGetComponent(out Weapons.Projectile proj))
            {
                // 알려주신 시그니처에 맞게 값을 정확히 매칭합니다.
                proj.Initialize(
                    direction: dir,                         // 1. 방향
                    speed: data.bulletSpeed,                // 2. 현재 속도
                    minSpeed: data.bulletSpeed,             // 3. 최소 속도 (감속 안 하면 speed와 동일하게)
                    damage: data.attackDamage,              // 4. 데미지
                    pierceCount: 0,                         // 5. 관통 횟수 (드론 탄환은 보통 0)
                    isPlayer: false,                        // 6. 적이 쏜 것이므로 false
                    stats: null
                );
            }
        }
    }
}