using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DroneEnemy : EnemyStats
    {
        private enum State { Moving, Shooting, Stunned, Staggered }
        private State currentState = State.Moving;

        private float stateTimer;

        protected override void Start()
        {
            base.Start();

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
            if (isStaggered)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void HandleMovingState()
        {
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            _rb.linearVelocity = dir * MoveSpeed.GetValue(); // data에서 가져옴

            if (stateTimer <= 0)
            {
                currentState = State.Shooting;
                stateTimer = data.shootDelay;
                _rb.linearVelocity = Vector2.zero;
                ShootAtPlayer();
            }
        }

        private void HandleShootingState()
        {
            // 발사 후 잠시 정지해 있는 상태
            _rb.linearVelocity = Vector2.zero;

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

            if (bullet.TryGetComponent(out Weapon.Projectile proj))
            {
                proj.Initialize(dir, new Weapon.ProjectileInfo{
                    damage = Mathf.RoundToInt(DamageIncreased.GetValue(AttackDamage.GetValue())), 
                    pierceCount = (int)Pierce.GetValue(), 
                    ricochetCount = (int)Ricochet.GetValue(),
                    homingRange = HomingRange.GetValue(), 
                    homingStrength = HomingStrength.GetValue(),
                    decelerationRate = DecelerationRate.GetValue(),
                    scale = ProjectileScale.GetValue(),
                    speed = ProjectileSpeed.GetValue(),
                    minSpeed = ProjectileSpeed.GetValue() / 10, 
                }, this);
            }
        }
    }
}