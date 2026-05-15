using System.Collections;
using UnityEngine;

namespace Drone
{
    public class Defense : MonoBehaviour
    {
        [Header("Defense Settings")]
        private float cooldownDuration = 3f; // 재사용 대기 시간
        private bool _isCoolingDown = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 적 탄환과 충돌 시 (레이어나 태그 활용)
            if (collision.CompareTag("EnemyProjectile"))
            {
                if (!_isCoolingDown)
                {
                    TriggerShockwave();
                    StartCoroutine(CooldownRoutine());
                }
            }
        }

        private void TriggerShockwave()
        {
            // 주변 탄환들 검색 (반경 3f)
            Collider2D[] bullets = Physics2D.OverlapCircleAll(
                transform.position,
                3f,
                1 << LayerMask.NameToLayer("EnemyProjectile")
            );
            foreach (var b in bullets)
            {
                Destroy(b.gameObject);
            }
        }

        private IEnumerator CooldownRoutine()
        {
            _isCoolingDown = true;
            yield return new WaitForSeconds(cooldownDuration);
            _isCoolingDown = false;
        }
    }
}
