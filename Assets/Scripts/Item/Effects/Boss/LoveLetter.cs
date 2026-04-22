using System.Collections;
using Entity;
using Player;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "LoveLetter", menuName = "Items/Boss/Love Letter")]
    public class LoveLetterItem : PassiveItemData
    {
        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<LoveLetterTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<LoveLetterTracker>();
                tracker.Initialize(player);
            }
        }
    }

    public class LoveLetterTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private bool _isSpent = false; // 발동 여부 (비활성화 플래그)

        public void Initialize(PlayerStats player)
        {
            _player = player;
            // 체력이 변할 때마다 체크하거나, 데미지를 입기 직전 이벤트를 구독합니다.
            _player.OnPreDamage += HandleItemEffect;
        }

        private void HandleItemEffect(ref int damage)
        {
            if (_isSpent)
                return;

            // 이번 데미지를 입으면 죽는지 체크
            if (_player.currentHealth - _player.DamageTaken.GetValue(damage) <= 0)
            {
                // 1. 데미지를 0으로 무효화 (이번 공격에서 살아남음)
                damage = -1;

                // 2. 효과 발동
                TriggerLetterEffect();
            }
        }

        private void TriggerLetterEffect()
        {
            _isSpent = true; // 아이템 비활성화

            // 1. 체력 100% 회복
            _player.Heal(_player.maxHealth);

            // 2. 3초간 무적 상태 돌입
            StartCoroutine(InvincibilityRoutine(3f));

            // (선택) 아이템 UI를 비활성화(회색)로 바꾸는 알림 전송
            // InventoryManager.Instance.DisableItemUI("LoverLetter");
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            // 기존에 만든 StatModifier 시스템 활용
            // 받는 피해량을 -100%로 만들어 데미지를 0으로 고정
            var invinceMod = new StatModifier(
                "LoverLetter_Invince",
                -1.0f,
                ModifierType.Percent,
                this
            );
            _player.DamageTaken.AddModifier(invinceMod);

            // 시각적 연출 (깜빡임 등)
            // _player.View.StartBlinking(duration);

            yield return new WaitForSeconds(duration);

            // 무적 해제
            _player.DamageTaken.RemoveModifiersFromSource(this);
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.OnPreDamage -= HandleItemEffect;
        }
    }
}
