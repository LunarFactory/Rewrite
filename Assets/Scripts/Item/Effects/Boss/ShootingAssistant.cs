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
        [SerializeField] private StatusEffectData buffData;
        public float bonusAttackSpeed = 0.04f;
        public int maxStack = 25;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ShootingAssistantTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ShootingAssistantTracker>();
                tracker.Initialize(player, buffData, bonusAttackSpeed, maxStack);
            }
        }
    }

    public class ShootingAssistantTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private AssistedEffect _buffData;
        private int _maxStack;

        public void Initialize(PlayerStats player, StatusEffectData buffData, float bonusAttackSpeed, int maxStack)
        {
            _player = player;
            _buffData = (AssistedEffect)buffData;
            _maxStack = maxStack;
            _buffData.bonusAttackSpeed = bonusAttackSpeed;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (attacker.TryGetComponent(out BuffManager buffManager))
            {
                if (_buffData != null)
                {
                    _buffData.setCurrentStack(Mathf.Min(_buffData.getCurrentStack() + 1, _maxStack));
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