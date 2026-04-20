using UnityEngine;
using System.Collections;
using Player;

namespace Item
{
    [CreateAssetMenu(fileName = "LifeSupportBattery", menuName = "Items/Life Support Battery")]
    public class LifeSupportBattery : PassiveItemData
    {
        [Header("Heal Settings")]
        public float healPercent = 0.75f;
        private bool _hasTriggered = false;

        // [핵심] 현재 실행 중인 코루틴을 저장할 변수
        private Coroutine _activeCoroutine;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 체력 변화 감지
            stats.OnHealthChanged += (currentHP) =>
            {
                bool isBelowThreshold = currentHP <= stats.maxHealth * 0.25f;

                // 1. 50% 이하이고, 아직 발동하지 않았다면 실행!
                if (isBelowThreshold && !_hasTriggered)
                {
                    stats.Heal(Mathf.RoundToInt(stats.maxHealth * healPercent));
                    _hasTriggered = true; // 깃발 올림
                }
            };
        }
    }
}