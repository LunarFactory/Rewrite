using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "OverchargedBattery", menuName = "Items/Uncommon/Overcharged Battery")]
    public class OverchargedBatteryItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Heal Settings")]
        public float damagePercent = 3f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<OverchargedBatteryTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<OverchargedBatteryTracker>();
                tracker.Initialize(player, damagePercent);
            }
        }
    }

    public class OverchargedBatteryTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damagePercent;

        public void Initialize(PlayerStats player, float damagePercent)
        {
            _player = player;
            _damagePercent = damagePercent;

            _player.OnPlayerApplyHardCC += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target)
        {
            if (target is EnemyStats enemy)
            {
                enemy.TakeDamage(attacker, Mathf.RoundToInt(attacker.AttackDamage.GetValue() * _damagePercent), Color.gold);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPlayerApplyHardCC -= HandleItemEffect;
            }
        }
    }
}