using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "IonMotor", menuName = "Items/Common/Ion Motor")]
    public class IonMotorItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Ion Boost Settings")]
        [SerializeField] private StatusEffectData buffData;
        public float cooldown = 15f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<IonMotorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<IonMotorTracker>();
                tracker.Initialize(player, buffData, cooldown);
            }
        }
    }

    public class IonMotorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private StatusEffectData _buffData;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, StatusEffectData buffData, float cooldown)
        {
            _player = player;
            _buffData = buffData;
            _cooldown = cooldown;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (!_isCooldown)
            {
                if (attacker.TryGetComponent(out BuffManager buffManager))
                {
                    if (_buffData != null)
                    {
                        buffManager.ApplyEffect(_buffData, 5, attacker);
                        StartCoroutine(CooldownRoutine());
                    }
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
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}