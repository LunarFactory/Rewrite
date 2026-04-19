using UnityEngine;
using Core;
using System;

namespace Entity
{
    public abstract class EntityStatus : MonoBehaviour
    {
        [Header("Common Stats")]
        public int maxHealth;
        public int currentHealth;
        
        // BuffManager나 아이템 시스템이 공통적으로 접근할 스탯들
        public CharacterStat AttackDamage;
        public CharacterStat MoveSpeed;

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
            if (_spriteRenderer != null) _originalColor = _spriteRenderer.color;
        }

        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        public virtual void Heal(int amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
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