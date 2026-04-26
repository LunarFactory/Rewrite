using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DefenseDroneAI : BaseDroneAI
    {
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

            HandleMovingState();
        }

        protected override void HandleMovingState()
        {
            base.HandleMovingState();
        }
    }
}
