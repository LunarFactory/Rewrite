using Enemy;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "EMP", menuName = "Items/Common/EMP")]
    public class EMPItem : PassiveItemData // PassiveItemData를 상속
    {
        [Header("EMP Settings")]
        public int maxStack = 10;
        public float duration = 0.5f;

        // BuffManager에 전달할 EMP 트리거용 SO (EMPEffectSO)
        [SerializeField]
        private EMPEffect empTriggerData;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<EMPTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<EMPTracker>();
                tracker.Initialize(player, empTriggerData, maxStack, duration);
            }
        }
    }

    public class EMPTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private EMPEffect _empTriggerData;

        public void Initialize(
            PlayerStats player,
            StatusEffectData empTriggerData,
            int maxStack,
            float duration
        )
        {
            _player = player;
            _empTriggerData = (EMPEffect)empTriggerData;
            _empTriggerData.maxStack = maxStack;
            _empTriggerData.duration = duration;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target == null)
                return;

            // 2. 적에게 BuffManager가 있는지 확인
            if (target.TryGetComponent(out BuffManager buffManager))
            {
                // 3. 인스펙터에서 할당한 EMP 트리거 데이터로 스택 1 추가
                // 이 스택이 10개가 되면 EMPEffectSO에 등록된 실제 기절 효과가 발동됩니다.
                if (!target.isStunned)
                {
                    buffManager.AddStack(_empTriggerData, 1, _player);
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
