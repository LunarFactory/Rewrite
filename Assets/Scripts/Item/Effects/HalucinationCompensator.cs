using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "HalucinationCompensator", menuName = "Items/Halucination Compensator")]
    public class HalucinationCompensator : PassiveItemData // 부모를 상속받음
    {
        [Header("Buff Settings")]
        public int bonusDamage = 5;    // 증가할 공격력 수치
        public float duration = 3f;
        private Coroutine _activeCoroutine;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 1. 플레이어의 벽 충돌 이벤트 구독
            stats.OnWallHit += () =>
            {
                // 2. 이미 버프가 작동 중이라면 기존 코루틴을 멈춰서 시간 초기화
                if (_activeCoroutine != null)
                {
                    stats.StopCoroutine(_activeCoroutine);
                }

                // 3. 버프 코루틴 시작
                _activeCoroutine = stats.StartCoroutine(BuffRoutine(stats));
            };

            Debug.Log($"{itemName} 효과가 적용되었습니다!");
        }

        private IEnumerator BuffRoutine(PlayerStats stats)
        {
            stats.AttackDamage.RemoveModifiersFromSource(this);
            // 4. 새로운 수정자 생성 (출처를 'this'로 지정하여 나중에 찾기 쉽게 함)
            StatModifier mod = new StatModifier(bonusDamage, ModifierType.Flat, this);

            // 5. 플레이어 공격력 스탯에 수정자 추가
            stats.AttackDamage.AddModifier(mod);
            Debug.Log($"<color=orange>[WallBuff]</color> 공격력 +{bonusDamage} 적용!");

            // 6. 지정된 시간만큼 대기
            yield return new WaitForSeconds(duration);

            // 7. 시간이 다 되면 '이 아이템(this)'이 추가한 수정자만 제거
            stats.AttackDamage.RemoveModifiersFromSource(this);
            Debug.Log("[WallBuff] 버프 지속시간 종료.");

            _activeCoroutine = null;
        }
    }
}