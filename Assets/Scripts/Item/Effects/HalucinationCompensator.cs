using UnityEngine;
using Player;
using Core;

namespace Item
{
    [CreateAssetMenu(fileName = "HalucinationCompensator", menuName = "Items/Halucination Compensator")]
    public class HalucinationCompensator : PassiveItemData
    {
        [Header("Buff Data")]
        [SerializeField] private StatusEffectData buffData;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 벽 충돌 이벤트 구독
            stats.OnWallHit += () =>
            {
                if (player.TryGetComponent(out BuffManager buffManager))
                {
                    if (buffData != null)
                    {
                        buffManager.ApplyEffect(buffData, 3);
                    }
                }
            };
        }
    }
}