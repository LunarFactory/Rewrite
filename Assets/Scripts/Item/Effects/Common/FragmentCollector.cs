using System.Collections;
using Core;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "FragmentCollector", menuName = "Items/Common/Fragment Collector")]
    public class FragmentCollectorItem : PassiveItemData
    {
        public float healPercent = 0.1f;
        public float cooldown = 3f;

        public override void OnApply(PlayerStats player)
        {
            // 플레이어에 트래커를 부착 (중복 획득 시 여러 개가 붙지 않게 체크)
            var tracker = player.GetComponent<FragmentCollectorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<FragmentCollectorTracker>();
                tracker.Initialize(player, healPercent, cooldown);
            }
        }
    }

    public class FragmentCollectorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _healPercent;
        private bool _isCooldown = false;
        private float _cooldown;

        public void Initialize(PlayerStats player, float healPercent, float cooldown)
        {
            _player = player;
            _healPercent = healPercent;
            _cooldown = cooldown;

            _player.OnKill += HandleItemEffect;
        }

        private void HandleItemEffect(Entity.EntityStats entity)
        {
            if (!_isCooldown)
            {
                _player.Heal(Mathf.RoundToInt(_player.maxHealth * _healPercent));
                StartCoroutine(CooldownRoutine());
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnKill -= HandleItemEffect;
            }
        }

        private IEnumerator CooldownRoutine()
        {
            _isCooldown = true;
            yield return new WaitForSeconds(_cooldown);
            _isCooldown = false;
        }
    }
}
