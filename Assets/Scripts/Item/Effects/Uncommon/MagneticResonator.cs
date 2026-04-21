using UnityEngine;
using System.Collections;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "MagneticResonator", menuName = "Items/Uncommon/Magnetic Resonator")]
    public class MagneticResonatorItem : PassiveItemData
    {
        public float range = 6f;

        // 이 효과가 이미 발동했는지 체크하는 변수
        public float cooldown = 15f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<MagneticResonatorTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<MagneticResonatorTracker>();
                tracker.Initialize(player, range, cooldown);
            }
        }
    }

    public class MagneticResonatorTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _range;
        private float _cooldown;

        private Coroutine currentCoroutine;

        public void Initialize(PlayerStats player, float range, float cooldown)
        {
            _player = player;
            _range = range;
            _cooldown = cooldown;

            currentCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            while (true) // 플레이어가 살아있는 동안 무한 반복
            {
                // 1. 효과 발동
                RemoveBulletEffect(transform.position);
                // 3. 15초 대기
                yield return new WaitForSeconds(_cooldown);
            }
        }

        private void RemoveBulletEffect(Vector2 center)
        {

            // 주변의 적 탄환 제거 및 적 기절 로직 (이전과 동일)
            int mask = LayerMask.GetMask("EnemyProjectile");
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, _range, mask);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("EnemyProjectile"))
                {
                    Destroy(hit.gameObject);
                }
            }

            // 시각적 피드백 (예: 쾅! 하는 파티클)을 여기서 생성하면 좋습니다.
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                StopCoroutine(currentCoroutine);
            }
        }
    }
}