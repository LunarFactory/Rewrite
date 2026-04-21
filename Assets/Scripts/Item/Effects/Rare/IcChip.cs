using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "IcChip", menuName = "Items/Rare/IC Chip")]
    public class IcChipItem : PassiveItemData // 부모를 상속받음
    {
        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<IcChipTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<IcChipTracker>();
                tracker.Initialize(player);
            }
        }
    }

    public class IcChipTracker : MonoBehaviour
    {
        private PlayerStats _player;

        public void Initialize(PlayerStats player)
        {
            _player = player;

            _player.OnPlayerPostAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target is EnemyStats entity)
            {
                int chipDamage = InventoryManager.Instance.CountItem();
                entity.TakeDamage(attacker, Mathf.RoundToInt(_player.DamageIncreased.GetValue(chipDamage)), Color.gold);
                _player.NotifyAttackHit(attacker, entity, chipDamage);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPlayerPostAttackHit -= HandleItemEffect;
            }
        }
    }
}