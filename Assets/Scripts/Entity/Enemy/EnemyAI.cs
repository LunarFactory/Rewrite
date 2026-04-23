using Player;
using UnityEngine;

namespace Enemy
{
    // 모든 AI의 부모가 될 추상 클래스
    [RequireComponent(typeof(EnemyStats))]
    public abstract class EnemyAI : MonoBehaviour
    {
        protected EnemyStats stats;
        protected Rigidbody2D rb;
        protected SpriteRenderer sr;
        protected EnemySpriteAnimationModule _animationModule;

        protected PlayerStats playerStat;
        protected Transform playerTarget;

        protected virtual void Awake()
        {
            stats = GetComponent<EnemyStats>();
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        protected virtual void Start()
        {
            _animationModule = GetComponent<EnemySpriteAnimationModule>();
            if (PlayerStats.LocalPlayer != null)
            {
                playerStat = PlayerStats.LocalPlayer;
                playerTarget = playerStat.transform;
            }
        }

        protected virtual void Update()
        {
            _animationModule.UpdateAnimation(Time.deltaTime);
            // 경직(Stagger) 중이거나 기절(Stun) 중이면 행동 중단
            if (stats.isStaggered || stats.isStunned)
            {
                StopBehavior();
                return;
            }
            ExecuteBehavior();
        }

        protected abstract void ExecuteBehavior(); // 자식들이 구현할 핵심 로직

        protected virtual void StopBehavior() // 행동 중지 시 처리 (예: 속도 0)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
