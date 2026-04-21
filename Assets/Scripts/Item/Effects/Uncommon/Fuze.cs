using UnityEngine;
using System.Collections;
using Player;
using Enemy;

namespace Item
{
    [CreateAssetMenu(fileName = "MagneticResonator", menuName = "Items/Common/Magnetic Resonator")]
    public class MagneticResonatorItem : PassiveItemData
    {
        public float range = 6f;
        public float threshold = 0.5f;

        // 이 효과가 이미 발동했는지 체크하는 변수
        public float cooldown = 30f;

        public override void OnApply(PlayerStats player)
        {
            var tracker = player.GetComponent<FuzeTracker>();
            if (tracker == null)
            {
                tracker = player.gameObject.AddComponent<FuzeTracker>();
                tracker.Initialize(player, effectToTrigger, range, stunDuration, threshold, cooldown);
            }
        }
    }

    public class FuzeTracker : MonoBehaviour
    {
        private PlayerStats _player;
        private float _range;
        private float _cooldown;
        private bool _isCooldown = false;

        public void Initialize(PlayerStats player, StatusEffectData effectToTrigger, float range, float stunDuration, float threshold, float cooldown)
        {
            _player = player;
            _range = range;
            _cooldown = cooldown;

            _player.OnHealthChanged += HandleItemEffect;
        }

        private void HandleItemEffect(int currentHealth)
        {
            if (!_isCooldown)
            {
                RemoveBulletEffect(_player.transform.position);
                StartCoroutine(CooldownRoutine());

            }
        }

        private void RemoveBulletEffect(Vector2 center)
        {

            // 주변의 적 탄환 제거 및 적 기절 로직 (이전과 동일)
            int mask = LayerMask.GetMask("Enemy", "EnemyBullet");
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

        private IEnumerator CooldownRoutine()
        {
            _isCooldown = true;
            yield return new WaitForSeconds(_cooldown);
            _isCooldown = false;
        }
        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnHealthChanged -= HandleItemEffect;
            }
        }
    }
}