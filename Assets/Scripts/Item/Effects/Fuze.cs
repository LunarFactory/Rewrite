using UnityEngine;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "Fuze", menuName = "Items/Fuze")]
    public class Fuze : PassiveItemData
    {
        public float range = 6f;
        public float stunDuration = 2f;
        
        // 이 효과가 이미 발동했는지 체크하는 변수
        private bool _hasTriggered = false;

        public override void OnApply(GameObject player, PlayerStats stats)
        {
            stats.OnHealthChanged += (currentHP) =>
            {
                bool isBelowThreshold = currentHP <= stats.MaxHealth * 0.5f;

                // 1. 50% 이하이고, 아직 발동하지 않았다면 실행!
                if (isBelowThreshold && !_hasTriggered)
                {
                    ExecuteCrisisEffect(player.transform.position);
                    _hasTriggered = true; // 깃발 올림
                }
                // 2. 만약 다시 체력을 50% 위로 회복했다면 깃발을 내림 (재장전)
                else if (!isBelowThreshold && _hasTriggered)
                {
                    _hasTriggered = false; // 다시 발동 가능 상태로 변경
                    Debug.Log("<color=blue>[위기 관리]</color> 장치 재충전 완료.");
                }
            };
        }

        private void ExecuteCrisisEffect(Vector2 center)
        {
            Debug.Log("<color=red>[위기 관리]</color> 1회성 충격파 발동!");

            // 주변의 적 탄환 제거 및 적 기절 로직 (이전과 동일)
            int mask = LayerMask.GetMask("Enemy", "EnemyBullet");
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, range, mask);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("EnemyProjectile"))
                {
                    Destroy(hit.gameObject);
                }

                var enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.Stun(stunDuration);
                }
            }
            
            // 시각적 피드백 (예: 쾅! 하는 파티클)을 여기서 생성하면 좋습니다.
        }
    }
}