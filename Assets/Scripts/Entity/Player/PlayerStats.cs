using UnityEngine;
using Core;
using System;
using Entity;
using Weapons;

namespace Player
{
    public class PlayerStats : EntityStatus // 상속 변경
    {
        [Header("Player Specifics")]
        public int baseAttackDamage = 10;
        public float baseAttackSpeed = 1;
        public float baseMoveSpeed = 5;
        public CharacterStat AttackSpeed;
        private PlayerStealth stealth;

        private WeaponBase currentWeapon;
        private WeaponData wData;

        // 이벤트들
        public delegate void PreDamageHandler(ref int damage);
        public event PreDamageHandler OnPreDamage;
        public event Action<float> OnHealthChanged; // float으로 변경 권장
        public event Action<float> OnPostDamage;
        public event Action OnWallHit;
        public event Action<Enemy.EnemyBase, float> OnAttackHit;

        private int bolts = 0;
        public static event Action<PlayerStats> OnPlayerReady;
        // 현재 활성화된 플레이어를 즉시 참조하기 위한 스태틱 변수
        public static PlayerStats LocalPlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LocalPlayer = this;
            // 씬이 로드되자마자 "나 여기 있다!"라고 알림
            OnPlayerReady?.Invoke(this);
        }
        protected virtual void Start()
        {
            maxHealth = 100; // 또는 데이터 시트 참조
            currentHealth = maxHealth;
            stealth = GetComponent<PlayerStealth>();
            currentWeapon = GetComponentInChildren<WeaponBase>();

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterPlayer(this);

            AttackDamage = new CharacterStat(baseAttackDamage); // 기본값
            AttackSpeed = new CharacterStat(baseAttackSpeed);
            MoveSpeed = new CharacterStat(baseMoveSpeed);
        }

        public override void TakeDamage(int damage)
        {
            if (stealth != null && (stealth.IsDodging || stealth.IsStealthActive)) return;

            int intDamage = (int)damage;
            if (intDamage > 0)
            {
                OnPreDamage?.Invoke(ref intDamage);
                if (intDamage <= 0) return;
            }

            base.TakeDamage(intDamage); // 부모 로직 실행 (currentHealth 감소 및 Die 체크)
            if (FDTManager.Instance != null)
            {
                // 적의 머리 위쪽에서 띄우고 싶다면 position + Vector3.up * 1f 처럼 오프셋을 줍니다.
                FDTManager.Instance.SpawnText(transform.position + Vector3.up * 0.5f, Mathf.RoundToInt(damage), Color.red);
            }
            OnHealthChanged?.Invoke(currentHealth);
            OnPostDamage?.Invoke(intDamage);
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
        public void NotifyAttackHit(Enemy.EnemyBase target, float damage) => OnAttackHit?.Invoke(target, damage);
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