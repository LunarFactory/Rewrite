using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Entity;
using Player;
using Weapon;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "EcoFriendlyRubberBullet", menuName = "Items/Rare/Eco Friendly Rubber Bullet")]
    public class EcoFriendlyRubberBulletItem : PassiveItemData // 부모를 상속받음
    {
        [Header("Heal Settings")]
        public int ricochetCount = 2;
        public float damageMultiplier = 1.2f;
        public float projectileScale = 1.2f;
        public float projectileSpeed = 1.2f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<EcoFriendlyRubberBulletTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<EcoFriendlyRubberBulletTracker>();
                tracker.Initialize(player, ricochetCount, damageMultiplier, projectileScale, projectileSpeed);
            }
        }
    }

    public class EcoFriendlyRubberBulletTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private int _ricochetCount;
        private float _damageMultiplier;
        private float _projectileScale;
        private float _projectileSpeed;
        private StatModifier _mod;
        public void Initialize(PlayerStats player, int ricochetCount, float damageMultiplier, float projectileScale, float projectileSpeed)
        {
            _player = player;
            _ricochetCount = ricochetCount;
            _damageMultiplier = damageMultiplier;
            _projectileScale = projectileScale;
            _projectileSpeed = projectileSpeed;

            _mod = new StatModifier("RecurrentNeuralNetworkRicochet", ricochetCount, ModifierType.Flat, this);
            _player.Ricochet.AddModifier(_mod);
            _player.OnWallHit += HandleItemEffect;
        }

        private void HandleItemEffect(Projectile proj)
        {
            proj.Damage = Mathf.RoundToInt(proj.Damage * _damageMultiplier);
            if (proj.CurrentSpeed < 40f)
            {
                proj.CurrentSpeed *= _projectileSpeed;
                if (proj.CurrentSpeed > 40f) proj.CurrentSpeed = 40f;
            }
            if (proj.transform.localScale.x < 2f)
            {
                proj.transform.localScale *= _projectileScale;
                if (proj.transform.localScale.x > 2f) proj.transform.localScale = new Vector3(2f, 2f, 2f);
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.Ricochet.RemoveModifiersFromSource(this);

                _player.OnWallHit -= HandleItemEffect;
            }
        }
    }
}