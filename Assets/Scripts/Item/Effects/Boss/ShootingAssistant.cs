using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "ShootingAssistant", menuName = "Items/Boss/Shooting Assistant")]
    public class ShootingAssistantItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Assisted Settings")]
        [SerializeField] private AssistedEffect buffData;
        public float bonusAttackSpeed = 0.04f;
        public float bonusDamageIncreased = 0.5f;
        public int maxStack = 25;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ShootingAssistantTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ShootingAssistantTracker>();
                tracker.Initialize(player, buffData, bonusAttackSpeed, bonusDamageIncreased, maxStack);
            }
        }
    }

    public class ShootingAssistantTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private AssistedEffect _buffData;

        public void Initialize(PlayerStats player, AssistedEffect buffData, float bonusAttackSpeed, float bonusDamageIncreased, int maxStack)
        {
            _player = player;
            _buffData = buffData;
            _buffData.maxStack = maxStack;
            _buffData.bonusAttackSpeed = bonusAttackSpeed;
            _buffData.bonusDamageIncreased = bonusDamageIncreased;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (attacker.TryGetComponent(out BuffManager buffManager))
            {
                if (_buffData != null)
                {
                    _buffData.setCurrentStack(Mathf.Min(_buffData.getCurrentStack() + 1, _buffData.maxStack));
                    buffManager.ApplyEffect(_buffData, 3, attacker);
                }
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}