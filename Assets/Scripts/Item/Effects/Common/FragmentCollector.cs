using UnityEngine;
using Player;
using Core;

namespace Item
{

    [CreateAssetMenu(fileName = "FragmentCollector", menuName = "Items/Common/Fragment Collector")]
    public class FragmentCollectorItem : PassiveItemData
    {
        public float healPercent = 0.05f;
        public override void OnApply(PlayerStats player)
        {
            // 플레이어에 트래커를 부착 (중복 획득 시 여러 개가 붙지 않게 체크)
            var tracker = player.GetComponent<FragmentCollectorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<FragmentCollectorTracker>();
                tracker.Initialize(player, healPercent);
            }
        }
    }
    public class FragmentCollectorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _healPercent;

        public void Initialize(PlayerStats player, float healPercent)
        {
            _player = player;
            _healPercent = healPercent;

            _player.OnKill += HandleItemEffect;
        }

        private void HandleItemEffect()
        {
            _player.Heal(Mathf.RoundToInt(_player.maxHealth * _healPercent));
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnKill -= HandleItemEffect;
            }
        }
    }
}
