using System;
using System.ComponentModel;
using Core;
using UnityEngine;

namespace Entity
{
    public abstract class EntityStats : MonoBehaviour
    {
        [Header("Common Stats")]
        public int maxHealth;
        public int currentHealth;

        // BuffManager나 아이템 시스템이 공통적으로 접근할 스탯들
        public CharacterStat AttackDamage;
        public CharacterStat MoveSpeed;
        public CharacterStat Ricochet;
        public CharacterStat Pierce;
        public CharacterStat HomingRange;
        public CharacterStat HomingStrength;
        public CharacterStat DecelerationRate;
        public CharacterStat ProjectileScale;
        public CharacterStat ProjectileSpeed;
        public CharacterStat DamageIncreased;
        public CharacterStat DamageTaken;
        public CharacterStat ReduceHeal;

        [Header("Common Components")]
        protected SpriteRenderer _spriteRenderer;
        protected Rigidbody2D _rb;
        protected Color _originalColor;

        public bool isDead = false;
        public bool isStunned = false;

        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        public virtual void TakeDamage(EntityStats attacker, int damage)
        {
            if (isDead)
                return;
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                isDead = true;
                currentHealth = 0;
                attacker.NotifyKill(this);
                Die();
            }
        }

        public virtual void NotifyAttackHit(
            EntityStats attacker,
            EntityStats entity,
            int damage
        ) { }

        public virtual void NotifyPostAttackHit(
            EntityStats attacker,
            EntityStats entity,
            int damage
        ) { }

        public virtual void NotifyKill(EntityStats entity) { }

        public virtual void NotifyHardCC(EntityStats attacker, EntityStats target) { }

        public virtual void NotifyPreHeal(EntityStats target, ref int amount) { }

        public virtual void NotifyHeal(EntityStats target, int amount) { }

        public virtual void NotifyOverHeal(EntityStats target, int amount) { }

        public virtual void NotifyPostHeal(EntityStats target, int amount) { }

        public virtual void Heal(int amount)
        {
            if (isDead)
                return;
            int healAmount = (int)ReduceHeal.GetValue(amount);
            NotifyPreHeal(this, ref healAmount);
            if (healAmount > maxHealth - currentHealth)
            {
                NotifyOverHeal(this, amount);
                healAmount = maxHealth - currentHealth;
            }
            currentHealth += healAmount;
            NotifyHeal(this, healAmount);
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(
                    transform.position + Vector3.up * 0.5f,
                    healAmount,
                    Color.green
                );
            }
            NotifyPostHeal(this, healAmount);
        }

        // 죽는 방식은 플레이어와 적이 완전히 다르므로 추상 메서드로 선언
        protected abstract void Die();

        public SpriteRenderer GetRenderer()
        {
            return _spriteRenderer;
        }

        public Color GetOriginalColor()
        {
            return _originalColor;
        }
    }
}
