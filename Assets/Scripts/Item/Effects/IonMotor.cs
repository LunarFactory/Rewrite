using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "IonMotor", menuName = "Items/Ion Motor")]
    public class IonMotor : PassiveItemData // 부모를 상속받음
    {
        [Header("Ion Boost Settings")]
        public float cooldown = 15f;
        [SerializeField] private StatusEffectData buffData;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            float lastFireTime = -999f;

            stats.OnAttackHit += (target, damage) =>
            {
                if (Time.time < lastFireTime + cooldown) return;
                Debug.Log("[이온 동력기] 가동 시작!");
                lastFireTime = Time.time;
                if (player.TryGetComponent(out BuffManager buffManager))
                {
                    if (buffData != null)
                    {
                        buffManager.ApplyEffect(buffData, 5);
                    }
                }
            };
        }
    }
}