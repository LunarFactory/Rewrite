using Enemy;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(
        fileName = "OverchargedBattery",
        menuName = "Items/Uncommon/Overcharged Battery"
    )]
    public class OverchargedBatteryItem : PassiveItemData // 부모를 상속받음
    {
        public float moveSpeedMultiplier = 0.5f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<OverchargedBatteryTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<OverchargedBatteryTracker>();
                tracker.Initialize(player, moveSpeedMultiplier);
            }
        }
    }

    public class OverchargedBatteryTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private StatModifier _moveSpeedMod;

        public void Initialize(PlayerStats player, float damageMultiplier)
        {
            _player = player;
            _moveSpeedMod = new StatModifier(
                "OverchargedBatteryMoveSpeed",
                damageMultiplier,
                ModifierType.Percent,
                this
            );

            _player.MoveSpeed.AddModifier(_moveSpeedMod);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.MoveSpeed.RemoveModifiersFromSource(this);
            }
        }
    }
}
