using System.Collections;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "Dynamite", menuName = "Items/Boss/Dynamite")]
    public class DynamiteItem : PassiveItemData
    {
        [Header("Heal Settings")]
        public float damageMultiplier = 2f;
        public float delayTime = 2f;
        public float explosionRadius = 3f;
        public float stunTime = 2f;
        public DelayedBombEffect delayedBombData;
        public StunEffect stunEffect;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<DynamiteTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<DynamiteTracker>();
                tracker.Initialize(
                    player,
                    delayedBombData,
                    damageMultiplier,
                    delayTime,
                    explosionRadius,
                    stunEffect,
                    stunTime
                );
            }
        }
    }

    public class DynamiteTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private DelayedBombEffect _delayedBombData;
        private float _delayTime;

        public void Initialize(
            PlayerStats player,
            DelayedBombEffect delayedBombData,
            float damageMultiplier,
            float delayTime,
            float explosionRadius,
            StunEffect stunEffect,
            float stunTime
        )
        {
            _player = player;
            _delayedBombData = delayedBombData;
            _delayedBombData.damageMultiplier = damageMultiplier;
            _delayedBombData.explosionRadius = explosionRadius;
            _delayedBombData.stunEffect = stunEffect;
            _delayedBombData.stunTime = stunTime;
            _delayTime = delayTime;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target.TryGetComponent<BuffManager>(out BuffManager buff))
            {
                if (buff.HasEffect(_delayedBombData) == null)
                {
                    buff.ApplyEffect(_delayedBombData, _delayTime, attacker);
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
