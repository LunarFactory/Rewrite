using UnityEngine;
using System.Collections;
using Core;

namespace Enemy
{
    public class EnemyBase : MonoBehaviour
    {
        [Header("Data Reference")]
        public EnemyData data; // 인스펙터에서 할당
        protected EnemySpriteAnimationModule _animator = new EnemySpriteAnimationModule();

        protected float currentHealth;
        public bool isDead = false;
        protected float staggerTimer;
        protected Transform playerTarget;
        private Coroutine _stunCoroutine;
        private Color _originalColor;
        protected SpriteRenderer spriteRenderer;
        protected Rigidbody2D rb;

        public bool IsStaggered => staggerTimer > 0f;
        public bool IsStunned = false;

        protected virtual void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();

            if (spriteRenderer != null) _originalColor = spriteRenderer.color;

            // 애니메이터 모듈 초기화 (참조 전달)
            _animator.Initialize(spriteRenderer, rb, data);

            if (data != null) currentHealth = data.maxHealth;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        protected virtual void Update()
        {
            if (staggerTimer > 0f) staggerTimer -= Time.deltaTime;

            // 애니메이터 모듈에게 "일 해라"라고 시킵니다.
            _animator.UpdateAnimation(Time.deltaTime);
        }

        private void FixedUpdate() // 오버라이드 불가능하게 private이나 sealed로!
        {
            if (IsStunned)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // 기절하지 않았을 때만 자식의 고유 로직을 실행하도록 시킴
            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate() {}

        public virtual void TakeDamage(float damage)
        {
            if (data == null) return;

            if (!data.isInvincible)
            {
                currentHealth -= damage;
            }

            staggerTimer = data.hitstunDuration;

            if (currentHealth <= 0 && !data.isInvincible)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;
            if (WaveManager.Instance != null)
            {
                GameManager.Instance.Player.AddBolts(100);
                WaveManager.Instance.OnEnemyDied();
            }
            Destroy(gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {

        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            // Initial impact damage
            if (collision.gameObject.CompareTag("Player") && data != null)
            {
                if (collision.gameObject.TryGetComponent(out Player.PlayerStats stats))
                {
                    stats.TakeDamage(data.attackDamage);
                }
            }
        }

        public void Stun(float duration)
        {
            // 이미 기절 중이라면 이전 기절 코루틴을 멈춤 (지속시간 갱신)
            if (_stunCoroutine != null)
            {
                StopCoroutine(_stunCoroutine);
            }

            _stunCoroutine = StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            IsStunned = true;

            // 시각적 피드백: 색상을 노랗거나 파랗게 변경
            if (spriteRenderer != null) spriteRenderer.color = Color.yellow;

            // AI나 이동 로직이 이 값을 체크해서 멈춰야 합니다.
            Debug.Log($"{gameObject.name} 기절됨!");

            yield return new WaitForSeconds(duration);

            // 기절 해제
            IsStunned = false;
            if (spriteRenderer != null) spriteRenderer.color = _originalColor;

            _stunCoroutine = null;
            Debug.Log($"{gameObject.name} 기절 풀림!");
        }
    }
}
