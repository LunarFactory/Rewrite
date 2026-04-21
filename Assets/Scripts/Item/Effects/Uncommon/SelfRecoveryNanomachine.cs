using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "SelfRecoveryNanomachine", menuName = "Items/Uncommon/Self Recovery Nanomachine")]
    public class SelfRecoveryNanomachineItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Heal Settings")]
        public int healAmount = 10;
        public float cooldown = 3;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<SelfRecoveryNanomachineTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<SelfRecoveryNanomachineTracker>();
                tracker.Initialize(player, healAmount, cooldown);
            }
        }
    }

    public class SelfRecoveryNanomachineTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private int _healAmount;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, int healAmount, float cooldown)
        {
            _player = player;
            _healAmount = healAmount;
            _cooldown = cooldown;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (!_isCooldown)
            {
                attacker.Heal(_healAmount);
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
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}