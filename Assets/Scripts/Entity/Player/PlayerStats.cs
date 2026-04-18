using UnityEngine;
using Core;
using System;

namespace Player
{
    public class PlayerStats : MonoBehaviour
    {
        public int MaxHealth = 100;
        public int currentHealth;
        [Header("Attack Stats")]
        public int baseAttackDamage = 10; // 플레이어 순수 공격력
        public CharacterStat AttackDamage;
        public float baseAttackSpeed = 1f; // 플레이어 순수 공격 속도
        public CharacterStat AttackSpeed;
        public float baseMoveSpeed = 5f;
        public CharacterStat MoveSpeed;
        private PlayerStealth stealth;
        public delegate void PreDamageHandler(ref int damage);
        public event PreDamageHandler OnPreDamage;
        public event Action<int> OnHealthChanged;
        public event Action<float> OnPostDamage;
        public event Action OnWallHit;

        [Header("Economy")]
        private int bolts = 0;

        public event Action<Enemy.EnemyBase, float> OnAttackHit;

        private void Start()
        {
            currentHealth = MaxHealth;
            stealth = GetComponent<PlayerStealth>();
            // 게임 시작 시 혹은 씬 진입 시 자신을 GameManager에 등록
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }
            AttackDamage = new CharacterStat(baseAttackDamage);
            AttackSpeed = new CharacterStat(baseAttackSpeed);
            MoveSpeed = new CharacterStat(baseMoveSpeed);
        }

        public void TakeDamage(int damage)
        {
            // 스텔스 활성 중 (회피 포함) 모든 피해 면역
            if (stealth != null && (stealth.IsDodging || stealth.IsStealthActive)) return;

            if (damage > 0)
            {
                OnPreDamage?.Invoke(ref damage);
                if (damage <= 0)
                {
                    Debug.Log("피격이 무효화되었습니다!");
                    return;
                }
            }
            currentHealth -= damage;
            Debug.Log($"<color=red>Player Hit!</color> Took {damage} damage. HP: {currentHealth}");
            OnHealthChanged?.Invoke(currentHealth); // 신호 발송
            OnPostDamage?.Invoke(damage);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void NotifyWallHit() => OnWallHit?.Invoke();
        public void Heal(int heal)
        {
            // 현재 체력 + 회복량이 MaxHealth를 넘지 않도록 제한
            currentHealth = Mathf.Min(currentHealth + heal, MaxHealth);

            Debug.Log($"<color=green>체력 회복됨!</color> HP: {currentHealth}/{MaxHealth}");
        }


        private void Die()
        {
            Debug.LogError("Game Over! Player HP reached 0. Restarting...");
            // RunManager 시드 재초기화 및 맵 갱신을 위해 씬 리로드
            GameManager.Instance?.ChangeState(GameManager.GameState.MainMenu);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public void NotifyAttackHit(Enemy.EnemyBase target, float damage)
        {
            OnAttackHit?.Invoke(target, damage);
        }
        public int GetCalculatedDamage(float weaponBaseDamageMultiplier)
        {
            // 플레이어 기초 피해량 * 무기 배수
            float damage = AttackDamage.GetValue() * weaponBaseDamageMultiplier;

            return Mathf.RoundToInt(damage);
        }
        public float GetCalculatedFireRate(float weaopnBaseFireRate)
        {
            // 플레이어 기초 피해량 * 무기 배수
            float fireRate = AttackSpeed.GetValue() * weaopnBaseFireRate;

            return fireRate;
        }
        public float GetCalculatedMoveSpeed()
        {
            // 플레이어 기초 피해량 * 무기 배수
            float speed = MoveSpeed.GetValue();

            return speed;
        }
        public int GetBolts()
        {
            return bolts;
        }
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
    }
}
