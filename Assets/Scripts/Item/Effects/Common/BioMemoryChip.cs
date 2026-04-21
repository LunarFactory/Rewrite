using UnityEngine;
using System.Collections;
using Player;

namespace Item
{
    [CreateAssetMenu(fileName = "BioMemoryChip", menuName = "Items/Common/Bio Memory Chip")]
    public class BioMemoryChipItem : PassiveItemData
    {
        [Header("Heal Settings")]
        public float healPercent = 0.2f;
        public float delayTime = 2f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<BioMemoryChipTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<BioMemoryChipTracker>();
                tracker.Initialize(player, healPercent, delayTime);
            }
        }
    }
    public class BioMemoryChipTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _healPercent;
        private float _delayTime;
        private Coroutine _activeCoroutine;

        public void Initialize(PlayerStats player, float healPercent, float delayTime)
        {
            _player = player;
            _healPercent = healPercent;
            _delayTime = delayTime;
            _activeCoroutine = null;

            _player.OnPostDamage += HandleItemEffect;
        }

        private void HandleItemEffect(int actualDamage)
        {
            if (_activeCoroutine != null)
            {
                _player.StopCoroutine(_activeCoroutine);
                Debug.Log("<color=yellow>[아이템]</color> 회복 타이머 초기화!");
            }

            // 2. 새로운 타이머 시작
            _activeCoroutine = _player.StartCoroutine(HealRoutine(_player));
        }

        private IEnumerator HealRoutine(PlayerStats player)
        {
            // 2초 대기 (이 도중에 다시 피격되면 StopCoroutine에 의해 취소됨)
            yield return new WaitForSeconds(_delayTime);

            // 대기가 끝났다면 (취소되지 않았다면) 회복 실행
            float lostHealth = player.maxHealth - player.currentHealth;
            if (lostHealth > 0)
            {
                int recovery = Mathf.RoundToInt(Mathf.Max(lostHealth * _healPercent, 1f));
                player.Heal(recovery);
            }

            // 코루틴이 끝났으므로 변수 비워주기
            _activeCoroutine = null;
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPostDamage -= HandleItemEffect;
            }
        }
    }
}