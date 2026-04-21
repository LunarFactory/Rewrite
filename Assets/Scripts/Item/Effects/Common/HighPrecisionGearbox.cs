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
        public StatusEffectData engagedEffect;
        public float engagedDuration = 3f;
        public float engagedDamagePercent = 0.5f;


        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<HighPrecisionGearboxTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<HighPrecisionGearboxTracker>();
                tracker.Initialize(player, engagedEffect, engagedDuration, engagedDamagePercent);
            }
        }
    }
    public class HighPrecisionGearboxTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private EngagedEffect _engagedEffect;
        private float _duration;
        private float _damagePercent;
        private readonly HashSet<EntityId> _hitTargets = new HashSet<EntityId>();

        public void Initialize(PlayerStats player, StatusEffectData engagedEffect, float engagedDuration, float engagedDamagePercent)
        {
            _player = player;
            _engagedEffect = (EngagedEffect)engagedEffect;
            _duration = engagedDuration;
            _damagePercent = engagedDamagePercent;

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
                    _engagedEffect.damage = Mathf.RoundToInt(_player.AttackDamage.GetValue() * _damagePercent);
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
