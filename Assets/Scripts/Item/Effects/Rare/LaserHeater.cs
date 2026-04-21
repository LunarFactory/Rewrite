using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "LaserHeater", menuName = "Items/Rare/Laser Heater")]
    public class LaserHeaterItem : PassiveItemData
    {
        [Header("Laser Heater Settings")]
        public HeatedEffect heatedEffect;
        public float heatedDuration = 3f;
        public float heatedDamageMultiplier = 0.6f;
        public float cooldown = 30f;


        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LaserHeaterTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LaserHeaterTracker>();
                tracker.Initialize(player, heatedEffect, heatedDuration, heatedDamageMultiplier, cooldown);
            }
        }
    }
    public class LaserHeaterTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private HeatedEffect _heatedEffect;
        private float _duration;
        private float _damageMultiplier;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, HeatedEffect heatedEffect, float heatedDuration, float heatedDamageMultiplier, float cooldown)
        {
            _player = player;
            _heatedEffect = heatedEffect;
            _duration = heatedDuration;
            _damageMultiplier = heatedDamageMultiplier;
            _cooldown = cooldown;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats player, EntityStats target, int damage)
        {

            if (!_isCooldown)
            {
                if (target == null || target.isDead) return;

                var buffManager = target.GetComponent<BuffManager>();
                if (buffManager != null && _heatedEffect != null)
                {
                    _heatedEffect.duration = _duration;
                    _heatedEffect.damage = Mathf.RoundToInt(_player.AttackDamage.GetValue() * _damageMultiplier);
                    buffManager.ApplyEffect(_heatedEffect, _heatedEffect.duration, player, true);
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
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}
