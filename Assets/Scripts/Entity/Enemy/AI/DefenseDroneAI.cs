using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DefenseDroneAI : EnemyAI
    {
        private enum State
        {
            Moving, // 플레이어를 추적하는 상태
        }

        [Header("Drone Settings")]
        [SerializeField]
        private float _stateTimer;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void ExecuteBehavior()
        {
            if (playerStat != null)
            {
                playerTarget = playerStat.isStealth() ? null : playerStat.transform;
            }

            if (playerTarget == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (stats.isStaggered)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * stats.MoveSpeed.GetValue();
        }
    }
}
