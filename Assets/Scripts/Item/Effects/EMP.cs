using UnityEngine;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "EMP", menuName = "Items/EMP")]
    public class EMP : PassiveItemData // PassiveItemData를 상속
    {
        [Header("EMP Settings")]
        // BuffManager에 전달할 EMP 트리거용 SO (EMPEffectSO)
        [SerializeField] private StatusEffectData empTriggerData;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 1. 공격 적중 이벤트 구독
            // (target: 맞은 대상, _: 데미지 등 추가 파라미터 무시)
            stats.OnAttackHit += (target, _) =>
            {
                if (target == null) return;

                // 2. 적에게 BuffManager가 있는지 확인
                if (target.TryGetComponent(out BuffManager buffManager))
                {
                    // 3. 인스펙터에서 할당한 EMP 트리거 데이터로 스택 1 추가
                    // 이 스택이 10개가 되면 EMPEffectSO에 등록된 실제 기절 효과가 발동됩니다.
                    if (!target.isStunned)
                    {
                        buffManager.AddStack(empTriggerData, 1);
                    }
                }
            };
        }
    }
}