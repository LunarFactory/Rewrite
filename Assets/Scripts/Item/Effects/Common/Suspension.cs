using UnityEngine;
using System.Collections;
using Player;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "Suspension", menuName = "Items/Common/Suspension")]
    public class SuspensionItem : PassiveItemData
    {
        [Header("Reduce Damage Data")]
        public float damageReducePercent = 0.5f;
        public float cooldown = 3f;

        public override void OnApply(PlayerStats player)
        {
            // 벽 충돌 이벤트 구독
            var tracker = player.GetComponent<SuspensionTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<SuspensionTracker>();
                tracker.Initialize(player, damageReducePercent, cooldown);
            }
        }
    }

    public class SuspensionTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damageReducePercent;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, float damageReducePercent, float cooldown)
        {
            _player = player;
            _damageReducePercent = damageReducePercent;
            _cooldown = cooldown;

            _player.OnPreDamage += HandleItemEffect;
        }

        private void HandleItemEffect(ref int damage)
        {
            if (!_isCooldown)
            {
                damage = Mathf.RoundToInt(damage * _damageReducePercent);
                if (FDTManager.Instance != null)
                {
                    FDTManager.Instance.SpawnText(transform.position + Vector3.up * 0.5f, "감쇄!", Color.gold);
                }
                StartCoroutine(CooldownRoutine());
            }
        }
        private IEnumerator CooldownRoutine()
        {
            _isCooldown = true;
            yield return new WaitForSeconds(_cooldown);
            _isCooldown = false;
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPreDamage -= HandleItemEffect;
            }
        }
    }
}