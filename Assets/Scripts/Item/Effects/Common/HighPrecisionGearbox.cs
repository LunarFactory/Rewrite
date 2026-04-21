using UnityEngine;
using System.Collections.Generic;
using Entity;
using Player;
using Enemy;

namespace Item
{

    [CreateAssetMenu(fileName = "HighPrecisionGearbox", menuName = "Items/Common/High Precision Gearbox")]
    public class HighPrecisionGearboxItem : PassiveItemData
    {
        [Header("Gearbox Settings")]
        public EngagedEffect engagedEffect;
        public float engagedDuration = 3f;
        public float engagedDamageMultiplier = 0.5f;


        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<HighPrecisionGearboxTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<HighPrecisionGearboxTracker>();
                tracker.Initialize(player, engagedEffect, engagedDuration, engagedDamageMultiplier);
            }
        }
    }
    public class HighPrecisionGearboxTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private EngagedEffect _engagedEffect;
        private float _duration;
        private float _damageMultiplier;
        private readonly HashSet<EntityId> _hitTargets = new HashSet<EntityId>();

        public void Initialize(PlayerStats player, EngagedEffect engagedEffect, float engagedDuration, float engagedDamageMultiplier)
        {
            _player = player;
            _engagedEffect = engagedEffect;
            _duration = engagedDuration;
            _damageMultiplier = engagedDamageMultiplier;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats player, EntityStats target, int damage)
        {
            if (target == null || target.isDead) return;

            EntityId targetID = target.gameObject.GetEntityId();

            // HashSet을 사용하면 마커 컴포넌트 없이도 "이미 맞은 적"인지 0.0001초만에 확인 가능
            if (!_hitTargets.Contains(targetID))
            {
                _hitTargets.Add(targetID);

                var buffManager = target.GetComponent<BuffManager>();
                if (buffManager != null && _engagedEffect != null)
                {
                    _engagedEffect.duration = _duration;
                    _engagedEffect.damage = Mathf.RoundToInt(_player.DamageIncreased.GetValue(_player.AttackDamage.GetValue() * _damageMultiplier));
                    buffManager.ApplyEffect(_engagedEffect, _engagedEffect.duration, player);
                }
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _hitTargets.Clear();
                _player.OnPlayerAttackHit -= HandleItemEffect;
            }
        }
    }
}
