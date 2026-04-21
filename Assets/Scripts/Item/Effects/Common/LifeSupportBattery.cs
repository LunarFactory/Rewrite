using UnityEngine;
using System.Collections;
using Player;

namespace Item
{
    [CreateAssetMenu(fileName = "LifeSupportBattery", menuName = "Items/Common/Life Support Battery")]
    public class LifeSupportBatteryItem : PassiveItemData
    {
        [Header("Heal Settings")]
        public float healPercent = 0.5f;
        public float threshold = 0.5f;
        public float cooldown = 30f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LifeSupportBatteryTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LifeSupportBatteryTracker>();
                tracker.Initialize(player, healPercent, threshold, cooldown);
            }
        }
    }

    public class LifeSupportBatteryTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _healPercent;
        private float _threshold;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, float healPercent, float threshold, float cooldown)
        {
            _player = player;
            _healPercent = healPercent;
            _threshold = threshold;
            _cooldown = cooldown;

            _player.OnHealthChanged += HandleItemEffect;
        }

        private void HandleItemEffect(int currentHealth)
        {
            if (!_isCooldown)
            {
                bool isBelowThreshold = currentHealth <= _player.maxHealth * _threshold;
                if (isBelowThreshold)
                {
                    _player.Heal(Mathf.RoundToInt(_player.maxHealth * _healPercent));
                    StartCoroutine(CooldownRoutine());
                }
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
                _player.OnHealthChanged -= HandleItemEffect;
            }
        }
    }
}