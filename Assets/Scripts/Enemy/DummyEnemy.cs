using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyEnemy : EnemyBase
    {
        public GameObject bulletPrefab;

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
            if (playerTarget == null) return;

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
                    ShootAtPlayer();
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
            GameObject prefab = bulletPrefab;
            if (prefab == null)
            {
                // Fallback attempt to get the generated prefab
                var testSetup = FindAnyObjectByType<Level.TestSetup>();
                if (testSetup != null) prefab = Level.TestSetup.BulletPrefab;
            }

            if (prefab == null) return;

            Vector2 dir = (playerTarget.position - transform.position).normalized;
            GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);
            bullet.SetActive(true);

#if UNITY_EDITOR
            if (bullet.TryGetComponent<SpriteRenderer>(out var sr))
            {
                var normSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/bullet/bullet_normal.png");
                if (normSprite != null) sr.sprite = normSprite;
            }
#endif
            
            if (!bullet.TryGetComponent(out Weapons.Projectile proj))
            {
                proj = bullet.AddComponent<Weapons.Projectile>();
            }
            proj.Initialize(dir, 10f, AttackDamage, 0, false); // speed=10, isPlayer=false
        }
    }
}
