using System;
using System.Collections.Generic;
using Core;
using Entity;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy
{
    public class EnemyStats : EntityStats // 상속 변경
    {
        [Header("Enemy Specifics")]
        public EnemyData data;

        [SerializeField]
        protected EnemySpriteAnimationModule _animationModule = new();

        protected float staggerTimer;
        public bool isBoss;
        protected Transform playerTarget;

        public bool isStaggered => staggerTimer > 0f;

        private BoxCollider2D col;

        private Dictionary<string, GameObject> _bulletDict = new Dictionary<string, GameObject>();

        public event Action<EnemyStats, EntityStats, int> OnEnemyAttackHit;
        public event Action<EnemyStats, EntityStats, int> OnEnemyPostAttackHit;
        public event Action<EnemyStats, EntityStats> OnEnemyApplyHardCC;

        protected override void Awake()
        {
            base.Awake(); // EntityStats의 Awake 실행 (SR, RB 참조 및 중력 설정)
        }

        protected void Start()
        {
            Setup(data);
        }

        /// <summary>
        /// [핵심] WaveManager에서 베이스 프리팹 생성 후 호출하는 메서드
        /// </summary>
        public void Setup(EnemyData newData)
        {
            this.AddComponent<BuffManager>();
            this.data = newData;

            // 1. EntityStats에 정의된 스탯들 초기화
            InitializeFromData(newData);

            // 2. 애니메이션 모듈 초기화
            _animationModule.Initialize(_spriteRenderer, _rb, data);

            // 3. 동적 AI 컴포넌트 부착 (EnemyData의 ComponentName 사용)
            AttachAIComponent(data.ComponentName);
            col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = data.colliderSize;
                col.offset = data.colliderOffset;
            }
            _bulletDict.Clear();
            foreach (var entry in data.bulletList)
            {
                if (!string.IsNullOrEmpty(entry.bulletKey) && entry.bulletPrefab != null)
                {
                    _bulletDict[entry.bulletKey] = entry.bulletPrefab;
                }
            }
        }

        private void InitializeFromData(EnemyData d)
        {
            // EntityStats의 필드들에 데이터 주입
            maxHealth = d.maxHealth;
            currentHealth = maxHealth;

            // CharacterStat 객체들 초기화 (기존 로직)
            AttackDamage = new CharacterStat(d.baseAttackDamage);
            MoveSpeed = new CharacterStat(d.baseMoveSpeed);
            Ricochet = new CharacterStat(d.baseRicochet);
            Pierce = new CharacterStat(d.basePierce);
            HomingRange = new CharacterStat(d.baseHomingRange);
            HomingStrength = new CharacterStat(d.baseHomingStrength);
            DecelerationRate = new CharacterStat(d.baseDecelerationRate);
            ProjectileScale = new CharacterStat(d.baseProjectileScale);
            ProjectileSpeed = new CharacterStat(d.baseProjectileSpeed);

            // 데미지 증가/감소 수치 초기화
            DamageIncreased = new CharacterStat(0);
            if (d.baseDamageIncreasedFlat != 0)
                DamageIncreased.AddModifier(
                    new StatModifier("baseFlat", d.baseDamageIncreasedFlat, ModifierType.Flat, this)
                );
            if (d.baseDamageIncreasedPercent != 0)
                DamageIncreased.AddModifier(
                    new StatModifier(
                        "basePct",
                        d.baseDamageIncreasedPercent,
                        ModifierType.Percent,
                        this
                    )
                );

            DamageTaken = new CharacterStat(0);
            if (d.baseDamageTakenFlat != 0)
                DamageTaken.AddModifier(
                    new StatModifier("baseFlat", d.baseDamageTakenFlat, ModifierType.Flat, this)
                );
            if (d.baseDamageTakenPercent != 0)
                DamageTaken.AddModifier(
                    new StatModifier(
                        "basePct",
                        d.baseDamageTakenPercent,
                        ModifierType.Percent,
                        this
                    )
                );

            ReduceHeal = new CharacterStat(0);
        }

        private void AttachAIComponent(string componentName)
        {
            // 현재 프로젝트의 모든 어셈블리를 뒤져서라도 타입을 찾아내는 좀 더 강력한 방법입니다.
            System.Type type = System.Type.GetType("Enemy." + componentName);

            if (type != null)
            {
                if (GetComponent(type) == null)
                {
                    gameObject.AddComponent(type);
                }
            }
        }

        protected virtual void Update()
        {
            if (staggerTimer > 0f)
                staggerTimer -= Time.deltaTime;

            // 애니메이션 모듈 업데이트 (움직임 방향 전달)
            _animationModule.UpdateAnimation(Time.deltaTime, _rb.linearVelocity.normalized);
        }

        public override void TakeDamage(EntityStats attacker, int damage)
        {
            if (data != null && data.isInvincible)
                return;

            base.TakeDamage(attacker, damage);
            staggerTimer = data != null ? data.hitstunDuration : 0f;
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(
                    transform.position + Vector3.up * 0.5f,
                    Mathf.RoundToInt(DamageTaken.GetValue(damage)),
                    Color.white
                );
            }
        }

        public void TakeDamage(EntityStats attacker, int damage, Color color)
        {
            if (data == null || data.isInvincible)
                return;
            int finalDamage = Mathf.RoundToInt(DamageTaken.GetValue(damage));
            base.TakeDamage(attacker, finalDamage); // 부모의 체력 감소 로직 실행
            staggerTimer = data.hitstunDuration;
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(
                    transform.position + Vector3.up * 0.5f,
                    finalDamage,
                    color
                );
            }
        }

        public override void NotifyAttackHit(EntityStats attacker, EntityStats target, int damage)
        {
            if (attacker is EnemyStats attack)
            {
                OnEnemyAttackHit?.Invoke(attack, target, damage);
            }
        }

        public override void NotifyPostAttackHit(
            EntityStats attacker,
            EntityStats target,
            int damage
        )
        {
            if (attacker is EnemyStats attack)
            {
                OnEnemyPostAttackHit?.Invoke(attack, target, damage);
            }
        }

        public override void NotifyHardCC(EntityStats attacker, EntityStats target)
        {
            if (attacker is EnemyStats)
            {
                OnEnemyApplyHardCC?.Invoke((EnemyStats)attacker, target);
            }
        }

        protected override void Die()
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnEnemyDied();

            Destroy(gameObject);
        }

        public GameObject GetBulletPrefab(string key)
        {
            if (_bulletDict.TryGetValue(key, out GameObject prefab))
            {
                return prefab;
            }
            return data.bulletList.Count > 0 ? data.bulletList[0].bulletPrefab : null; // 못 찾으면 기본 탄환
        }
    }
}
