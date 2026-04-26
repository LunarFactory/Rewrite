using System;
using Enemy;
using Entity;
using Log;
using UI;
using UnityEngine;
using Weapon;

namespace Player
{
    public class PlayerStats : EntityStats // 상속 변경
    {
        [Header("Player Specifics")]
        public int baseAttackDamage = 10;
        public float baseAttackSpeed = 1;
        public float baseMoveSpeed = 5;
        public int baseRicochet = 0;
        public int basePierce = 0;
        public float baseHomingRange = 0f;
        public float baseHomingStrength = 0f;
        public float baseDecelerationRate = 1f;
        public float baseProjectileScale = 1f;
        public float baseProjectileSpeed = 1f;
        public float baseDamageIncreasedFlat = 0f;
        public float baseDamageIncreasedPercent = 0f;
        public float baseDamageTakenFlat = 0f;
        public float baseDamageTakenPercent = 0f;
        public CharacterStat AttackSpeed;
        private PlayerStealth stealth;

        private WeaponBase currentWeapon;

        // 이벤트들
        public delegate void PreDamageHandler(ref int damage);
        public event PreDamageHandler OnPreDamage;
        public event Action<int> OnHealthChanged;
        public event Action<EntityStats, int> OnHeal;
        public event Action<EntityStats, int> OnOverHeal;
        public event Action<int> OnPostDamage;
        public event Action<Projectile> OnWallHit;
        public event Action<EntityStats> OnKill;
        public event Action<PlayerStats, EntityStats> OnPlayerApplyHardCC;

        private int bolts = 0;
        public static event Action<PlayerStats> OnPlayerReady;
        public event Action<PlayerStats, EntityStats, int> OnPlayerAttackHit;
        public event Action<PlayerStats, EntityStats, int> OnPlayerPostAttackHit;

        // 현재 활성화된 플레이어를 즉시 참조하기 위한 스태틱 변수
        public static PlayerStats LocalPlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LocalPlayer = this;
            maxHealth = 100; // 또는 데이터 시트 참조
            currentHealth = maxHealth;
            stealth = GetComponent<PlayerStealth>();

            AttackDamage = new CharacterStat(baseAttackDamage); // 기본값
            AttackSpeed = new CharacterStat(baseAttackSpeed);
            MoveSpeed = new CharacterStat(baseMoveSpeed);
            Ricochet = new CharacterStat(baseRicochet);
            Pierce = new CharacterStat(basePierce);
            HomingRange = new CharacterStat(baseHomingRange);
            HomingStrength = new CharacterStat(baseHomingStrength);
            DecelerationRate = new CharacterStat(baseDecelerationRate);
            ProjectileScale = new CharacterStat(baseProjectileScale);
            ProjectileSpeed = new CharacterStat(baseProjectileSpeed);
            DamageIncreased = new CharacterStat(0);
            if (baseDamageIncreasedFlat != 0)
                DamageIncreased.AddModifier(
                    new StatModifier(
                        "baseDamageIncreasedFlat",
                        baseDamageIncreasedFlat,
                        ModifierType.Flat,
                        this
                    )
                );
            if (baseDamageIncreasedPercent != 0)
                DamageIncreased.AddModifier(
                    new StatModifier(
                        "baseDamageIncreasedPercent",
                        baseDamageIncreasedPercent,
                        ModifierType.Percent,
                        this
                    )
                );
            DamageTaken = new CharacterStat(0);
            if (baseDamageTakenFlat != 0)
                DamageTaken.AddModifier(
                    new StatModifier(
                        "baseDamageTakenFlat",
                        baseDamageTakenFlat,
                        ModifierType.Flat,
                        this
                    )
                );
            if (baseDamageTakenPercent != 0)
                DamageTaken.AddModifier(
                    new StatModifier(
                        "baseDamageTakenPercent",
                        baseDamageTakenPercent,
                        ModifierType.Percent,
                        this
                    )
                );
            ReduceHeal = new CharacterStat(0);

            currentWeapon = GetComponentInChildren<WeaponBase>();
            // 씬이 로드되자마자 "나 여기 있다!"라고 알림
            OnPlayerReady?.Invoke(this);
        }

        public override void TakeDamage(EntityStats attacker, int damage)
        {
            if (stealth != null && stealth.IsStealthActive)
                return;
            int finalDamage = Mathf.RoundToInt(DamageTaken.GetValue(damage));

            if (finalDamage > 0)
            {
                OnPreDamage?.Invoke(ref finalDamage);
                if (finalDamage < 0)
                    return;
            }

            base.TakeDamage(attacker, finalDamage); // 부모 로직 실행 (currentHealth 감소 및 Die 체크)
            LogTracker.Instance.RegisterHitTaken();
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(
                    transform.position + Vector3.up * 0.5f,
                    finalDamage,
                    Color.red
                );
            }
            OnHealthChanged?.Invoke(currentHealth);
            OnPostDamage?.Invoke(finalDamage);
        }

        public int GetWeaponBaseAttackDamage()
        {
            return Mathf.RoundToInt(
                AttackDamage.GetValue() * currentWeapon.weaponData.damageMultiplier
            );
        }

        public float GetWeaponBaseAttackSpeed()
        {
            return AttackSpeed.GetValue() * currentWeapon.weaponData.fireRate;
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            OnHealthChanged?.Invoke(currentHealth);
        }

        protected override void Die(EntityStats source) // 추상 메서드 구현
        {
            if (source is EnemyStats enemy)
            {
                LogTracker.Instance.EndWaveAndSend(0.5f, 0.5f, 0.5f);
                LogTracker.Instance.OnRunEnded("GAME_OVER", enemy.data.enemyName);
            }
            UIManager.Instance.RequestStateChange(UIState.GameOver);
        }

        public override void NotifyAttackHit(EntityStats attacker, EntityStats target, int damage)
        {
            if (attacker is PlayerStats player)
            {
                LogTracker.Instance.RegisterHit();
                OnPlayerAttackHit?.Invoke(player, target, damage);
            }
        }

        public override void NotifyPostAttackHit(
            EntityStats attacker,
            EntityStats target,
            int damage
        )
        {
            if (attacker is PlayerStats)
            {
                OnPlayerPostAttackHit?.Invoke((PlayerStats)attacker, target, damage);
            }
        }

        public override void NotifyHeal(EntityStats target, int amount)
        {
            OnHeal?.Invoke(target, amount);
        }

        public override void NotifyOverHeal(EntityStats target, int amount)
        {
            OnOverHeal?.Invoke(target, amount);
        }

        public override void NotifyKill(EntityStats entity)
        {
            OnKill?.Invoke(entity);
        }

        public override void NotifyHardCC(EntityStats attacker, EntityStats target)
        {
            if (attacker is PlayerStats)
            {
                OnPlayerApplyHardCC?.Invoke((PlayerStats)attacker, target);
            }
        }

        // 경제 및 유틸리티 메서드들...
        public void NotifyWallHit(Projectile proj) => OnWallHit?.Invoke(proj);

        public bool AddBolts(int amount)
        {
            bolts += amount;
            // 여기에 볼트 UI 업데이트 이벤트를 넣으면 좋습니다.
            return true;
        }

        public int GetBolts()
        {
            return bolts;
        }

        public bool isStealth()
        {
            return stealth.IsStealthActive;
        }

        public WeaponData GetWeaponData()
        {
            return currentWeapon.weaponData;
        }
    }
}
