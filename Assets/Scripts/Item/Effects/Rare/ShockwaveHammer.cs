using System.Collections.Generic;
using Enemy;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "ShockwaveHammer", menuName = "Items/Rare/Shockwave Hammer")]
    public class ShockwaveHammerItem : PassiveItemData
    {
        [Header("Shockwave Settings")]
        public float damageMultiplier = 0.5f;
        public float shockwaveRadius = 2f; // 충격파 범위

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<ShockwaveHammerTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<ShockwaveHammerTracker>();
                tracker.Initialize(player, damageMultiplier, shockwaveRadius);
            }
        }
    }

    public class ShockwaveHammerTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _damageMultiplier; // 50%
        private float _shockwaveRadius; // 충격파 범위

        public void Initialize(PlayerStats player, float damageMultiplier, float shockwaveRadius)
        {
            _player = player;
            _damageMultiplier = damageMultiplier;
            _shockwaveRadius = shockwaveRadius;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                target.transform.position,
                _shockwaveRadius,
                LayerMask.GetMask("Enemy")
            );

            foreach (var col in hitEnemies)
            {
                if (col.TryGetComponent<EnemyStats>(out var enemy))
                {
                    // 3. 범위 내 적들에게 피해 입힘 (Color.gold로 크리티컬 느낌 강조)
                    enemy.TakeDamage(
                        attacker,
                        Mathf.RoundToInt(damage * _damageMultiplier),
                        Color.gold
                    );
                }
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnPlayerAttackHit -= HandleItemEffect;
        }
    }
}
