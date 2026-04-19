using UnityEngine;
using Core;
using System;
using Entity;

namespace Enemy
{
    public class EnemyBase : EntityStatus // 상속 변경
    {
        [Header("Enemy Specifics")]
        public EnemyData data;
        protected EnemySpriteAnimationModule _animator = new EnemySpriteAnimationModule();

        protected float staggerTimer;
        protected Transform playerTarget;

        public bool isStaggered => staggerTimer > 0f;

        private GameObject player;
        private Player.PlayerStats stat;

        protected virtual void Start()
        {
            // 부모의 Awake에서 이미 컴포넌트를 잡았으므로 초기화만 진행
            _animator.Initialize(_spriteRenderer, _rb, data);

            if (data != null)
            {
                maxHealth = data.maxHealth;
                currentHealth = maxHealth;
                AttackDamage = new CharacterStat(data.baseAttackDamage);
                MoveSpeed = new CharacterStat(data.baseMoveSpeed);
            }

            player = GameObject.FindGameObjectWithTag("Player");
            stat = player.GetComponent<Player.PlayerStats>();
            if (player != null) playerTarget = player.transform;
        }

        protected virtual void Update()
        {
            if (staggerTimer > 0f) staggerTimer -= Time.deltaTime;
            _animator.UpdateAnimation(Time.deltaTime);
            if (stat.isStealth()) playerTarget = null;
            else playerTarget = player.transform;
        }

        private void FixedUpdate()
        {
            if (isStunned)
            {
                if (_rb.bodyType != RigidbodyType2D.Static) _rb.linearVelocity = Vector2.zero;
                return;
            }
            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate() { }

        public override void TakeDamage(int damage)
        {
            if (data != null && data.isInvincible) return;

            base.TakeDamage(damage); // 부모의 체력 감소 로직 실행
            staggerTimer = data.hitstunDuration;
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(transform.position + Vector3.up * 0.5f, Mathf.RoundToInt(damage), Color.white);
            }
        }

        protected override void Die() // 추상 메서드 구현
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
    }
}