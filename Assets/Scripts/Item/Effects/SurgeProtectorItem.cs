using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "SurgeProtector", menuName = "Items/Surge Protector")]
    public class SurgeProtector : PassiveItemData // 부모를 상속받음
    {
        [Header("Shield Settings")]
        public float cooldown = 5f;

        private bool _isShieldReady = false;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            // 아이템을 먹는 순간 쿨타임 코루틴 시작
            stats.StartCoroutine(ShieldRoutine());

            stats.OnPreDamage += (ref int damage) =>
            {
                if (_isShieldReady)
                {
                    damage = 0;            // 데미지를 0으로 만듦
                    _isShieldReady = false; // 실드 소모

                    // 시각 효과 처리 (예: 실드 파괴 파티클)
                    CreateShieldVisual(player.transform);
                }
            };

            Debug.Log($"{itemName} 효과가 적용되었습니다!");
        }

        private IEnumerator ShieldRoutine()
        {
            while (true)
            {
                if (!_isShieldReady)
                {
                    yield return new WaitForSeconds(cooldown);
                    _isShieldReady = true;
                    Debug.Log("<color=blue>[Shield]</color> 실드 충전 완료!");
                }
                yield return null;
            }
        }
        private void CreateShieldVisual(Transform playerTransform)
        {
            // 여기에 실드가 터지는 파티클이나 효과음을 넣으면 좋습니다.
        }
    }
}