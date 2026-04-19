using UnityEngine;
using System.Collections;
using Player;

namespace Item
{
    [CreateAssetMenu(fileName = "BioMemoryChip", menuName = "Items/Bio Memory Chip")]
    public class BioMemoryChip : PassiveItemData
    {
        [Header("Heal Settings")]
        public float healPercent = 0.2f;
        public float delayTime = 2f;

        // [핵심] 현재 실행 중인 코루틴을 저장할 변수
        private Coroutine _activeCoroutine;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 피격 신호 구독
            stats.OnPostDamage += (actualDamage) =>
            {
                // 1. 만약 이미 실행 중인 회복 타이머가 있다면 취소 (초기화)
                if (_activeCoroutine != null)
                {
                    stats.StopCoroutine(_activeCoroutine);
                    Debug.Log("<color=yellow>[아이템]</color> 회복 타이머 초기화!");
                }

                // 2. 새로운 타이머 시작
                _activeCoroutine = stats.StartCoroutine(HealRoutine(stats));
            };
        }

        private IEnumerator HealRoutine(PlayerStats stats)
        {
            // 2초 대기 (이 도중에 다시 피격되면 StopCoroutine에 의해 취소됨)
            yield return new WaitForSeconds(delayTime);

            // 대기가 끝났다면 (취소되지 않았다면) 회복 실행
            float lostHealth = stats.maxHealth - stats.currentHealth;
            if (lostHealth > 0)
            {
                int recovery = Mathf.RoundToInt(Mathf.Max(lostHealth * healPercent, 1f));
                stats.Heal(recovery);
            }

            // 코루틴이 끝났으므로 변수 비워주기
            _activeCoroutine = null;
        }
    }
}