using UnityEngine;
using Player;
using Weapon;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "HologramProjector", menuName = "Items/Uncommon/Hologram Projector")]
    public class HologramProjectorItem : PassiveItemData
    {
        [Header("Homing Data")]
        public float homingRange = 10f;
        public float homingStrength = 10f;
        public int ricochetCount = 1;

        public override void OnApply(PlayerStats player)
        {
            // 벽 충돌 이벤트 구독
            var tracker = player.GetComponent<HologramProjectorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<HologramProjectorTracker>();
                tracker.Initialize(player, homingRange, homingStrength, ricochetCount);
            }
        }
    }

    public class HologramProjectorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _homingRange;
        private float _homingStrength;
        private StatModifier _mod;

        public void Initialize(PlayerStats player, float homingRange, float homingStrength, int ricochetCount)
        {
            _player = player;
            _homingRange = homingRange;
            _homingStrength = homingStrength;

            _mod = new StatModifier("HologramProjectorRicochet", ricochetCount, ModifierType.Flat, this);
            _player.Ricochet.AddModifier(_mod);
            _player.OnWallHit += HandleItemEffect;
        }

        private void HandleItemEffect(Projectile proj)
        {
            proj.HomingRange += _homingRange;
            proj.HomingStrength += _homingStrength;
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