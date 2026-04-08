using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DummyEnemy : EnemyBase
    {
        private Rigidbody2D rb;

        protected override void Start()
        {
            base.Start();
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            if (playerTarget != null)
            {
                Vector2 dir = (playerTarget.position - transform.position).normalized;
                rb.linearVelocity = dir * MoveSpeed;
            }
        }
    }
}
