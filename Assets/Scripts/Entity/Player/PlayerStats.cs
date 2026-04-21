using UnityEngine;
using Core;
using System;
using Entity;
using Weapons;

namespace Player
{
    public class PlayerStats : EntityStats // 상속 변경
    {
        [Header("Player Specifics")]
        public int baseAttackDamage = 10;
        public float baseAttackSpeed = 1;
        public float baseMoveSpeed = 5;
        public float baseDamageIncreasedFlat = 0f;
        public float baseDamageIncreasedPercent = 0f;
        public float baseDamageTakenFlat = 0f;
        public float baseDamageTakenPercent = 0f;
        public CharacterStat AttackSpeed;
        private PlayerStealth stealth;

        private WeaponBase currentWeapon;
        private WeaponData wData;

        // 이벤트들
        public delegate void PreDamageHandler(ref int damage);
        public event PreDamageHandler OnPreDamage;
        public event Action<int> OnHealthChanged; // float으로 변경 권장
        public event Action<int> OnPostDamage;
        public event Action OnWallHit;
        public event Action OnKill;

        private int bolts = 0;
        public static event Action<PlayerStats> OnPlayerReady;
        public event Action<PlayerStats, EntityStats, int> OnPlayerAttackHit;
        // 현재 활성화된 플레이어를 즉시 참조하기 위한 스태틱 변수
        public static PlayerStats LocalPlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LocalPlayer = this;
            // 씬이 로드되자마자 "나 여기 있다!"라고 알림
            OnPlayerReady?.Invoke(this);
            maxHealth = 100; // 또는 데이터 시트 참조
            currentHealth = maxHealth;
            stealth = GetComponent<PlayerStealth>();
            currentWeapon = GetComponentInChildren<WeaponBase>();

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterPlayer(this);

            AttackDamage = new CharacterStat(baseAttackDamage); // 기본값
            AttackSpeed = new CharacterStat(baseAttackSpeed);
            MoveSpeed = new CharacterStat(baseMoveSpeed);
            DamageIncreased = new CharacterStat(0);
            if (baseDamageIncreasedFlat != 0) DamageIncreased.AddModifier(new StatModifier("baseDamageIncreasedFlat", baseDamageIncreasedFlat, ModifierType.Flat, this));
            if (baseDamageIncreasedPercent != 0) DamageIncreased.AddModifier(new StatModifier("baseDamageIncreasedPercent", baseDamageIncreasedPercent, ModifierType.Percent, this));
            DamageTaken = new CharacterStat(0);
            if (baseDamageTakenFlat != 0) DamageTaken.AddModifier(new StatModifier("baseDamageTakenFlat", baseDamageTakenFlat, ModifierType.Flat, this));
            if (baseDamageTakenPercent != 0) DamageTaken.AddModifier(new StatModifier("baseDamageTakenPercent", baseDamageTakenPercent, ModifierType.Percent, this));
            ApplyUpgrades();
        }
        private void ApplyUpgrades()
        {
            foreach (var data in UpgradeManager.Instance.allUpgrades)
            {
                int level = UpgradeManager.Instance.GetLevel(data.id);
                if (level <= 0) continue;

                float bonusValue = data.statOffsets[level - 1];

                // 기존의 StatModifier 시스템 활용
                StatModifier metaMod = new StatModifier("MetaUpgrade", bonusValue, ModifierType.Percent, this);

                switch (data.statType)
                {
                    case StatType.Damage: AttackDamage.AddModifier(metaMod); break;
                    case StatType.AttackSpeed: AttackSpeed.AddModifier(metaMod); break;
                    case StatType.MoveSpeed: MoveSpeed.AddModifier(metaMod); break;
                }
            }
        }
        public override void TakeDamage(EntityStats attacker, int damage)
        {
            if (stealth != null && (stealth.IsDodging || stealth.IsStealthActive)) return;

            if (damage > 0)
            {
                OnPreDamage?.Invoke(ref damage);
                if (damage <= 0) return;
            }

            base.TakeDamage(attacker, damage); // 부모 로직 실행 (currentHealth 감소 및 Die 체크)
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(transform.position + Vector3.up * 0.5f, Mathf.RoundToInt(damage), Color.red);
            }
            OnHealthChanged?.Invoke(currentHealth);
            OnPostDamage?.Invoke(damage);
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);
            OnHealthChanged?.Invoke(currentHealth);
        }

        protected override void Die() // 추상 메서드 구현
        {
            Debug.LogError("Player Died!");
            GameManager.Instance?.ChangeState(GameManager.GameState.MainMenu);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public override void NotifyAttackHit(EntityStats attacker, EntityStats target, int damage)
        {
            if (attacker is PlayerStats)
            {
                OnPlayerAttackHit?.Invoke((PlayerStats)attacker, target, damage);
            }
        }
        public override void NotifyKill()
        {
            AddBolts(100);
            OnKill?.Invoke();
        }

        public int GetCalculatedAttackDamage()
        {
            wData = currentWeapon.weaponData;
            return Mathf.RoundToInt(AttackDamage.GetValue() * wData.damageMultiplier);
        }

        public float GetCalculatedAttackSpeed()
        {
            wData = currentWeapon.weaponData;
            return AttackSpeed.GetValue() * wData.FireRate;
        }

        // 경제 및 유틸리티 메서드들...
        public void NotifyWallHit() => OnWallHit?.Invoke();
        public bool AddBolts(int amount)
        {
            if (amount > 0)
            {
                bolts += amount;
                Debug.Log($"볼트 획득! 현재 잔액: {bolts}");
                // 여기에 볼트 UI 업데이트 이벤트를 넣으면 좋습니다.
                return true;
            }
            else
            {
                if (bolts >= amount)
                {
                    bolts -= amount;
                    return true;
                }
                return false;
            }
        }
        public int GetBolts()
        {
            return bolts;
        }

        public bool isStealth()
        {
            return stealth.IsStealthActive;
        }
    }
}