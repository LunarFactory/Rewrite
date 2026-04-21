using UnityEngine;
using Entity;
using Player;
using Enemy;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "HeavyMagazine", menuName = "Items/Rare/HeavyMagazine")]
    public class HeavyMagazineItem : PassiveItemData
    {
        [Header("Homing Data")]
        public float damageMultiplier = 0.5f;
        public float scale = 1f;
        public float speed = -0.3f;

        public override void OnApply(PlayerStats player)
        {
            // 벽 충돌 이벤트 구독
            var tracker = player.GetComponent<HeavyMagazineTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<HeavyMagazineTracker>();
                tracker.Initialize(player, damageMultiplier, scale, speed);
            }
        }
    }

    public class HeavyMagazineTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damageMultiplier;
        private StatModifier _scale;
        private StatModifier _speed;

        public void Initialize(PlayerStats player, float damageMultiplier, float scale, float speed)
        {
            _player = player;
            _damageMultiplier = damageMultiplier;

            _scale = new StatModifier("HeavyMagazineProjectileScale", scale, ModifierType.Percent, this);
            _speed = new StatModifier("HeavyMagazineProjectileSpeed", speed, ModifierType.Percent, this);

            _player.ProjectileScale.AddModifier(_scale);
            _player.ProjectileSpeed.AddModifier(_speed);

            _player.OnPlayerPostAttackHit += HandleItemEffect;
        }
        private void HandleItemEffect(PlayerStats attacker, EntityStats entity, int damage)
        {
            if (entity is EnemyStats enemy)
            {
                enemy.TakeDamage(attacker, Mathf.RoundToInt(attacker.DamageIncreased.GetValue(attacker.GetWeaponBaseAttackDamage() * _damageMultiplier)), Color.gold);
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.ProjectileScale.RemoveModifiersFromSource(this);
                _player.ProjectileSpeed.RemoveModifiersFromSource(this);
                _player.OnPlayerPostAttackHit -= HandleItemEffect;
            }
        }
    }
}