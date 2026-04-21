using UnityEngine;
using Entity;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "LaserInscriber", menuName = "Items/Uncommon/Laser Inscriber")]
    public class LaserInscriberItem : PassiveItemData // PassiveItemData를 상속
    {
        [Header("EMP Settings")]
        public int maxStack = 3;
        // BuffManager에 전달할 EMP 트리거용 SO (EMPEffectSO)
        [SerializeField] private StatusEffectData laserTriggerData;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LaserInscriberTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LaserInscriberTracker>();
                tracker.Initialize(player, laserTriggerData, maxStack);
            }
        }
    }

    public class LaserInscriberTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private LaserEffect _laserTriggerData;

        public void Initialize(PlayerStats player, StatusEffectData laserTriggerData, int maxStack)
        {
            _player = player;
            _laserTriggerData = (LaserEffect)laserTriggerData;
            _laserTriggerData.maxStack = maxStack;

            _player.OnPlayerAttackHit += HandleItemEffect;
        }

        private void HandleItemEffect(PlayerStats attacker, EntityStats target, int damage)
        {
            if (target == null) return;

            // 2. 적에게 BuffManager가 있는지 확인
            if (target.TryGetComponent(out BuffManager buffManager))
            {
                // 3. 인스펙터에서 할당한 EMP 트리거 데이터로 스택 1 추가
                // 이 스택이 10개가 되면 EMPEffectSO에 등록된 실제 기절 효과가 발동됩니다.
                if (!target.isStunned)
                {
                    buffManager.AddStack(_laserTriggerData, 1, _player);
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