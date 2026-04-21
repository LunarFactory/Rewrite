using UnityEngine;
using Player;
using Weapon;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "HalucinationCompensator", menuName = "Items/Common/Halucination Compensator")]
    public class HalucinationCompensatorItem : PassiveItemData
    {
        [Header("Buff Data")]
        [SerializeField] private StatusEffectData buffData;

        public override void OnApply(PlayerStats player)
        {
            // 벽 충돌 이벤트 구독
            var tracker = player.GetComponent<HalucinationCompensatorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<HalucinationCompensatorTracker>();
                tracker.Initialize(player, buffData);
            }
        }
    }

    public class HalucinationCompensatorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private StatusEffectData _buffData;

        public void Initialize(PlayerStats player, StatusEffectData buffData)
        {
            _player = player;
            _buffData = buffData;

            _player.OnWallHit += HandleItemEffect;
        }

        private void HandleItemEffect(Projectile proj)
        {
            if (_player.TryGetComponent(out BuffManager buffManager))
            {
                if (_buffData != null)
                {
                    buffManager.ApplyEffect(_buffData, 3, _player);
                }
            }
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnWallHit -= HandleItemEffect;
            }
        }
    }
}