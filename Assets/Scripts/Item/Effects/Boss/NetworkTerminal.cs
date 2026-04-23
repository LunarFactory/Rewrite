using System;
using System.Collections;
using Core;
using Enemy;
using Entity;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "NetworkTerminal", menuName = "Items/Boss/Network Terminal")]
    public class NetworkTerminalItem : PassiveItemData
    {
        public float cooldown = 30f;
        public float damageMultiplier = 1.0f;
        public float damageValue = 0.2f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<NetworkTerminalTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<NetworkTerminalTracker>();
                tracker.Initialize(player, damageMultiplier, damageValue, cooldown);
            }
        }
    }

    public class NetworkTerminalTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damageMultiplier;
        private float _damageValue;
        private float _cooldown;

        public void Initialize(
            PlayerStats player,
            float damageMultiplier,
            float damageValue,
            float cooldown
        )
        {
            _player = player;
            _damageMultiplier = damageMultiplier;
            _damageValue = damageValue;
            _cooldown = cooldown;

            GameManager.OnBossSummon += HandleBossDamage;
            _player.OnPlayerAttackHit += HandleBossExtraDamage;
        }

        private void HandleBossDamage(EntityStats boss)
        {
            StartCoroutine(BossDamage(boss));
        }

        private void HandleBossExtraDamage(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target is EnemyStats enemy)
            {
                if (enemy.isBoss)
                {
                    enemy.TakeDamage(
                        attacker,
                        Mathf.RoundToInt(
                            _player.DamageIncreased.GetValue(damage * _damageMultiplier)
                        ),
                        Color.aliceBlue
                    );
                }
            }
        }

        private IEnumerator BossDamage(EntityStats boss)
        {
            yield return new WaitForSeconds(_cooldown);
            if (boss is EnemyStats enemy)
            {
                enemy.TakeDamage(
                    _player,
                    Mathf.RoundToInt(enemy.maxHealth * _damageValue),
                    Color.aliceBlue
                );
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.DamageTaken.RemoveModifiersFromSource(this);

                GameManager.OnBossSummon -= HandleBossDamage;
                _player.OnPlayerAttackHit -= HandleBossExtraDamage;
            }
        }
    }
}
