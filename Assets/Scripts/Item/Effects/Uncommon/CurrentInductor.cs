using UnityEngine;
using Player;
using Weapon;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "CurrentInductor", menuName = "Items/Uncommon/Current Inductor")]
    public class CurrentInductorItem : PassiveItemData
    {
        [Header("Homing Data")]
        public float homingRange = 5f;
        public float homingStrength = 5f;
        public int pierceCount = 1;

        public override void OnApply(PlayerStats player)
        {
            // 벽 충돌 이벤트 구독
            var tracker = player.GetComponent<CurrentInductorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<CurrentInductorTracker>();
                tracker.Initialize(player, homingRange, homingStrength, pierceCount);
            }
        }
    }

    public class CurrentInductorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private StatModifier _homingRange;
        private StatModifier _homingStrength;
        private StatModifier _pierce;

        public void Initialize(PlayerStats player, float homingRange, float homingStrength, int pierceCount)
        {
            _player = player;

            _homingRange = new StatModifier("CurrentInductorHomingRange", homingRange, ModifierType.Flat, this);
            _homingStrength = new StatModifier("CurrentInductorHomingStrength", homingStrength, ModifierType.Flat, this);
            _pierce = new StatModifier("CurrentInductorPierce", pierceCount, ModifierType.Flat, this);

            _player.Pierce.AddModifier(_pierce);
            _player.HomingRange.AddModifier(_homingRange);
            _player.HomingStrength.AddModifier(_homingStrength);
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.Pierce.RemoveModifiersFromSource(this);
                _player.HomingRange.RemoveModifiersFromSource(this);
                _player.HomingStrength.RemoveModifiersFromSource(this);
            }
        }
    }
}