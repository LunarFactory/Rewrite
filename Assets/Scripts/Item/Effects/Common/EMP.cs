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
        public float moveSpeed = -0.2f;
        public float duration = 2f;

        // BuffManager에 전달할 EMP 트리거용 SO (EMPEffectSO)
        [SerializeField]
        private EMPEffect empTriggerData;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<EMPTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<EMPTracker>();
                tracker.Initialize(player, empTriggerData, moveSpeed, duration);
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
            float moveSpeed,
            float duration
        )
        {
            _player = player;
            _empTriggerData = (EMPEffect)empTriggerData;
            _empTriggerData.moveSpeed = moveSpeed;
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
                buffManager.ApplyEffect(_empTriggerData, _empTriggerData.duration, _player);
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
