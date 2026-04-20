using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "SelfRecoveryNanomachine", menuName = "Items/Self Recovery Nanomachine")]
    public class SelfRecoveryNanomachine : PassiveItemData // 부모를 상속받음
    {
        [Header("Heal Settings")]
        public int healAmount = 10;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            stats.OnAttackHit += (target, damage) =>
            {
                stats.Heal(healAmount);
            };
        }
    }
}