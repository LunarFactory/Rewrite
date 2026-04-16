using UnityEngine;
using UI;

namespace Enemy
{
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Display Settings")]
        public string bossDisplayName = "M3-K4";

        protected override void Start()
        {
            base.Start();
            Debug.Log($"[BossEnemy] {bossDisplayName} 소환됨 - UI 표시 요청");

            // Notify UI to show the boss health bar
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.ShowBossHP(bossDisplayName, "");
                GameHUD.Instance.SetBossHP(currentHealth / MaxHealth);
            }

            // Subscribe to health changes to update UI
            OnHealthChanged += UpdateBossUI;
            OnDeath += ClearBossUI;
        }

        private void UpdateBossUI(float ratio)
        {
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.SetBossHP(ratio);
            }
        }

        private void ClearBossUI()
        {
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.HideBossHP();
            }
        }

        protected override void Die()
        {
            // Clear UI before destruction
            ClearBossUI();
            base.Die();
        }

        private void OnDestroy()
        {
            // Ensure UI is hidden if boss is destroyed for any other reason
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.HideBossHP();
            }
        }
    }
}
