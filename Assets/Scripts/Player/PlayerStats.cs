using UnityEngine;
using Core;

namespace Player
{
    public class PlayerStats : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float currentHealth;
        
        private PlayerStealth stealth;

        private void Start()
        {
            currentHealth = MaxHealth;
            stealth = GetComponent<PlayerStealth>();
        }

        public void TakeDamage(float damage)
        {
            // 스텔스 활성 중 (회피 포함) 모든 피해 면역
            if (stealth != null && (stealth.IsDodging || stealth.IsStealthActive)) return;
            
            currentHealth -= damage;
            Debug.Log($"<color=red>Player Hit!</color> Took {damage} damage. HP: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.LogError("Game Over! Player HP reached 0. Restarting...");
            // RunManager 시드 재초기화 및 맵 갱신을 위해 씬 리로드
            GameManager.Instance?.ChangeState(GameManager.GameState.MainMenu);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
